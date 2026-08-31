-- ═══════════════════════════════════════════════════
-- Attendly - Supabase PostgreSQL Database Schema
-- ═══════════════════════════════════════════════════

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ── Profiles / Users ──
CREATE TABLE IF NOT EXISTS student_profiles (
 id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
 email TEXT UNIQUE NOT NULL,
 password_hash TEXT NOT NULL,
 full_name TEXT,
 college TEXT,
 course TEXT,
 year TEXT,
 division TEXT,
 railway_line TEXT,
 station TEXT,
 bio TEXT,
 profile_image_url TEXT,
 role TEXT DEFAULT 'student' CHECK (role IN ('student', 'admin')),
 is_premium BOOLEAN DEFAULT FALSE,
 premium_expires_at TIMESTAMPTZ,
 railway_line_group TEXT,
 is_online BOOLEAN DEFAULT FALSE,
 last_seen TIMESTAMPTZ,
 is_active BOOLEAN DEFAULT TRUE,
 is_verified BOOLEAN DEFAULT FALSE,
 created_at TIMESTAMPTZ DEFAULT NOW(),
 updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_profiles_email ON student_profiles(email);
CREATE INDEX idx_profiles_railway_line ON student_profiles(railway_line);
CREATE INDEX idx_profiles_is_active ON student_profiles(is_active);

-- ── Subjects ──
CREATE TABLE IF NOT EXISTS subjects (
 id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
 user_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 name TEXT NOT NULL,
 code TEXT,
 professor TEXT,
 room TEXT,
 lectures_per_week INTEGER DEFAULT 3,
 target_percentage INTEGER DEFAULT 75 CHECK (target_percentage BETWEEN 0 AND 100),
 color TEXT DEFAULT '#3B82F6',
 is_archived BOOLEAN DEFAULT FALSE,
 created_at TIMESTAMPTZ DEFAULT NOW(),
 updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_subjects_user_id ON subjects(user_id);

-- ── Attendance Records ──
CREATE TABLE IF NOT EXISTS attendance_records (
 id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
 user_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 subject_id UUID NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
 date DATE NOT NULL,
 status TEXT NOT NULL DEFAULT 'present' CHECK (status IN ('present', 'absent', 'cancelled', 'late', 'excused')),
 lecture_number INTEGER,
 notes TEXT,
 created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_attendance_user_subject ON attendance_records(user_id, subject_id);
CREATE INDEX idx_attendance_date ON attendance_records(date);

-- ── Timetable ──
CREATE TABLE IF NOT EXISTS timetable_entries (
 id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
 user_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 subject_id UUID NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
 day_of_week INTEGER NOT NULL CHECK (day_of_week BETWEEN 1 AND 7),
 start_time TIME NOT NULL,
 end_time TIME NOT NULL,
 room TEXT,
 created_at TIMESTAMPTZ DEFAULT NOW(),
 updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_timetable_user_id ON timetable_entries(user_id);

-- ── Calendar Events ──
CREATE TABLE IF NOT EXISTS calendar_events (
 id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
 user_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 title TEXT NOT NULL,
 date DATE NOT NULL,
 event_type TEXT DEFAULT 'personal' CHECK (event_type IN ('holiday', 'exam', 'personal', 'college')),
 description TEXT,
 is_all_day BOOLEAN DEFAULT TRUE,
 created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_calendar_user_date ON calendar_events(user_id, date);

-- ── Holidays ──
CREATE TABLE IF NOT EXISTS holidays (
 id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
 name TEXT NOT NULL,
 date DATE NOT NULL,
 region TEXT,
 type TEXT DEFAULT 'public' CHECK (type IN ('public', 'college', 'regional')),
 is_active BOOLEAN DEFAULT TRUE,
 created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_holidays_date ON holidays(date);

-- ── Connections ──
CREATE TABLE IF NOT EXISTS connections (
 id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
 requester_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 receiver_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 status TEXT DEFAULT 'pending' CHECK (status IN ('pending', 'accepted', 'rejected', 'blocked')),
 conversation_id UUID,
 created_at TIMESTAMPTZ DEFAULT NOW(),
 updated_at TIMESTAMPTZ,
 responded_at TIMESTAMPTZ,
 UNIQUE(requester_id, receiver_id)
);

CREATE INDEX idx_connections_requester ON connections(requester_id);
CREATE INDEX idx_connections_receiver ON connections(receiver_id);
CREATE INDEX idx_connections_status ON connections(status);

-- ── Conversations ──
CREATE TABLE IF NOT EXISTS conversations (
 id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
 user1_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 user2_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 created_at TIMESTAMPTZ DEFAULT NOW(),
 last_message_at TIMESTAMPTZ,
 UNIQUE(user1_id, user2_id)
);

CREATE INDEX idx_conversations_users ON conversations(user1_id, user2_id);

-- ── Messages ──
CREATE TABLE IF NOT EXISTS messages (
 id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
 conversation_id UUID NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
 sender_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 content TEXT NOT NULL,
 created_at TIMESTAMPTZ DEFAULT NOW(),
 read_at TIMESTAMPTZ,
 is_deleted BOOLEAN DEFAULT FALSE
);

CREATE INDEX idx_messages_conversation ON messages(conversation_id, created_at);

-- ── Reports ──
CREATE TABLE IF NOT EXISTS reports (
 id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
 reporter_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 reported_user_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 message_id UUID,
 reason TEXT NOT NULL,
 description TEXT,
 status TEXT DEFAULT 'pending' CHECK (status IN ('pending', 'reviewed', 'dismissed', 'actioned')),
 reviewed_by_admin_id UUID REFERENCES student_profiles(id),
 admin_notes TEXT,
 created_at TIMESTAMPTZ DEFAULT NOW(),
 reviewed_at TIMESTAMPTZ
);

CREATE INDEX idx_reports_status ON reports(status);

-- ── Subscriptions ──
CREATE TABLE IF NOT EXISTS subscriptions (
 id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
 user_id UUID NOT NULL REFERENCES student_profiles(id) ON DELETE CASCADE,
 razorpay_order_id TEXT NOT NULL,
 razorpay_payment_id TEXT NOT NULL,
 plan TEXT DEFAULT 'monthly' CHECK (plan IN ('monthly', 'yearly')),
 discount_applied INTEGER DEFAULT 0,
 status TEXT DEFAULT 'active' CHECK (status IN ('active', 'expired', 'cancelled')),
 amount_paid DECIMAL(10,2) NOT NULL,
 started_at TIMESTAMPTZ NOT NULL,
 expires_at TIMESTAMPTZ NOT NULL,
 created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_subscriptions_user_id ON subscriptions(user_id);
CREATE INDEX idx_subscriptions_status ON subscriptions(status);

-- ── Insert Sample Holidays ──
INSERT INTO holidays (name, date, type) VALUES
 ('Republic Day', '2026-01-26', 'public'),
 ('Mahashivratri', '2026-02-17', 'public'),
 ('Holi', '2026-03-06', 'public'),
 ('Gudi Padwa', '2026-03-22', 'public'),
 ('Ram Navami', '2026-04-01', 'public'),
 ('Mahavir Jayanti', '2026-04-08', 'public'),
 ('Buddha Purnima', '2026-05-03', 'public'),
 ('Independence Day', '2026-08-15', 'public'),
 ('Ganesh Chaturthi', '2026-09-04', 'public'),
 ('Diwali', '2026-11-01', 'public'),
 ('Christmas', '2026-12-25', 'public')
 ON CONFLICT DO NOTHING;

-- ── Insert Sample Admin ──
-- Password hash for "admin123"
INSERT INTO student_profiles (email, password_hash, full_name, role, is_verified)
 VALUES ('admin@attendly.app', '$2a$10$rQ8E8x8E8x8E8x8E8x8E8O', 'Admin User', 'admin', TRUE)
 ON CONFLICT (email) DO NOTHING;
