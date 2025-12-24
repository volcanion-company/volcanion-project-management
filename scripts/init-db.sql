-- =====================================================
-- Volcanion Project Management - Database Initialization
-- =====================================================
-- This script creates the database schema, tables, and seed data
-- It runs automatically when the postgres container starts for the first time
-- =====================================================

-- Create database (if not exists)
SELECT 'CREATE DATABASE volcanionpm'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'volcanionpm')\gexec

-- Connect to the database
\c volcanionpm;

-- =====================================================
-- SCHEMA CREATION
-- =====================================================

-- Create Organizations table
CREATE TABLE IF NOT EXISTS organizations (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    street VARCHAR(200),
    city VARCHAR(100),
    state VARCHAR(100),
    country VARCHAR(100),
    postal_code VARCHAR(20),
    is_active BOOLEAN NOT NULL DEFAULT true,
    subscription_tier VARCHAR(50),
    subscription_start_date TIMESTAMP,
    subscription_end_date TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    CONSTRAINT chk_org_name_not_empty CHECK (name <> '')
);

-- Create indexes for Organizations
CREATE INDEX IF NOT EXISTS idx_organizations_name ON organizations(name);
CREATE INDEX IF NOT EXISTS idx_organizations_is_active ON organizations(is_active);
CREATE INDEX IF NOT EXISTS idx_organizations_subscription_tier ON organizations(subscription_tier);

-- Create Users table
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY,
    organization_id UUID NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL,
    email_confirmed BOOLEAN NOT NULL DEFAULT false,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    profile_picture_url VARCHAR(500),
    phone_number VARCHAR(20),
    failed_login_attempts INTEGER NOT NULL DEFAULT 0,
    last_failed_login_at TIMESTAMP,
    lockout_end TIMESTAMP,
    last_login_at TIMESTAMP,
    refresh_token VARCHAR(500),
    refresh_token_expires_at TIMESTAMP,
    password_reset_token VARCHAR(10),
    password_reset_token_expires_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at TIMESTAMP,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(100),
    CONSTRAINT fk_users_organization FOREIGN KEY (organization_id) REFERENCES organizations(id) ON DELETE RESTRICT,
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT chk_user_email_format CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$')
);

-- Create indexes for Users
CREATE INDEX IF NOT EXISTS idx_users_organization_id ON users(organization_id);
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_role ON users(role);
CREATE INDEX IF NOT EXISTS idx_users_is_active ON users(is_active);
CREATE INDEX IF NOT EXISTS idx_users_refresh_token ON users(refresh_token);
CREATE INDEX IF NOT EXISTS idx_users_organization_role ON users(organization_id, role);

-- Create Projects table
CREATE TABLE IF NOT EXISTS projects (
    id UUID PRIMARY KEY,
    organization_id UUID NOT NULL,
    project_manager_id UUID,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    status VARCHAR(50) NOT NULL,
    priority VARCHAR(50) NOT NULL,
    start_date TIMESTAMP,
    end_date TIMESTAMP,
    budget_amount DECIMAL(18,2),
    budget_currency VARCHAR(3),
    actual_cost DECIMAL(18,2) NOT NULL DEFAULT 0,
    progress_percentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    CONSTRAINT fk_projects_organization FOREIGN KEY (organization_id) REFERENCES organizations(id) ON DELETE RESTRICT,
    CONSTRAINT fk_projects_manager FOREIGN KEY (project_manager_id) REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT chk_project_progress CHECK (progress_percentage >= 0 AND progress_percentage <= 100),
    CONSTRAINT chk_project_budget CHECK (budget_amount >= 0),
    CONSTRAINT chk_project_dates CHECK (end_date IS NULL OR start_date IS NULL OR end_date >= start_date)
);

-- Create indexes for Projects
CREATE INDEX IF NOT EXISTS idx_projects_organization_id ON projects(organization_id);
CREATE INDEX IF NOT EXISTS idx_projects_manager_id ON projects(project_manager_id);
CREATE INDEX IF NOT EXISTS idx_projects_status ON projects(status);
CREATE INDEX IF NOT EXISTS idx_projects_priority ON projects(priority);
CREATE INDEX IF NOT EXISTS idx_projects_dates ON projects(start_date, end_date);
CREATE INDEX IF NOT EXISTS idx_projects_org_status ON projects(organization_id, status);

-- Create Tasks table
CREATE TABLE IF NOT EXISTS tasks (
    id UUID PRIMARY KEY,
    project_id UUID NOT NULL,
    parent_task_id UUID,
    assigned_to_id UUID,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    status VARCHAR(50) NOT NULL,
    priority VARCHAR(50) NOT NULL,
    task_type VARCHAR(50) NOT NULL,
    story_points INTEGER,
    estimated_hours DECIMAL(10,2),
    actual_hours DECIMAL(10,2) NOT NULL DEFAULT 0,
    due_date TIMESTAMP,
    completed_date TIMESTAMP,
    is_blocked BOOLEAN NOT NULL DEFAULT false,
    blocked_reason TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    CONSTRAINT fk_tasks_project FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
    CONSTRAINT fk_tasks_parent FOREIGN KEY (parent_task_id) REFERENCES tasks(id) ON DELETE SET NULL,
    CONSTRAINT fk_tasks_assigned_to FOREIGN KEY (assigned_to_id) REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT chk_task_story_points CHECK (story_points >= 0),
    CONSTRAINT chk_task_hours CHECK (estimated_hours >= 0 AND actual_hours >= 0)
);

-- Create indexes for Tasks
CREATE INDEX IF NOT EXISTS idx_tasks_project_id ON tasks(project_id);
CREATE INDEX IF NOT EXISTS idx_tasks_parent_id ON tasks(parent_task_id);
CREATE INDEX IF NOT EXISTS idx_tasks_assigned_to ON tasks(assigned_to_id);
CREATE INDEX IF NOT EXISTS idx_tasks_status ON tasks(status);
CREATE INDEX IF NOT EXISTS idx_tasks_priority ON tasks(priority);
CREATE INDEX IF NOT EXISTS idx_tasks_type ON tasks(task_type);
CREATE INDEX IF NOT EXISTS idx_tasks_due_date ON tasks(due_date);
CREATE INDEX IF NOT EXISTS idx_tasks_project_status ON tasks(project_id, status);
CREATE INDEX IF NOT EXISTS idx_tasks_assigned_status ON tasks(assigned_to_id, status);

-- Create Sprints table
CREATE TABLE IF NOT EXISTS sprints (
    id UUID PRIMARY KEY,
    project_id UUID NOT NULL,
    name VARCHAR(200) NOT NULL,
    goal TEXT,
    status VARCHAR(50) NOT NULL,
    start_date TIMESTAMP,
    end_date TIMESTAMP,
    total_story_points INTEGER NOT NULL DEFAULT 0,
    completed_story_points INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    CONSTRAINT fk_sprints_project FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
    CONSTRAINT chk_sprint_story_points CHECK (total_story_points >= 0 AND completed_story_points >= 0),
    CONSTRAINT chk_sprint_dates CHECK (end_date IS NULL OR start_date IS NULL OR end_date >= start_date)
);

-- Create indexes for Sprints
CREATE INDEX IF NOT EXISTS idx_sprints_project_id ON sprints(project_id);
CREATE INDEX IF NOT EXISTS idx_sprints_status ON sprints(status);
CREATE INDEX IF NOT EXISTS idx_sprints_dates ON sprints(start_date, end_date);

-- Create TimeEntries table
CREATE TABLE IF NOT EXISTS time_entries (
    id UUID PRIMARY KEY,
    task_id UUID NOT NULL,
    user_id UUID NOT NULL,
    hours DECIMAL(10,2) NOT NULL,
    entry_date TIMESTAMP NOT NULL,
    description TEXT,
    is_billable BOOLEAN NOT NULL DEFAULT true,
    entry_type VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    CONSTRAINT fk_timeentries_task FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE CASCADE,
    CONSTRAINT fk_timeentries_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT chk_timeentry_hours CHECK (hours > 0)
);

-- Create indexes for TimeEntries
CREATE INDEX IF NOT EXISTS idx_timeentries_task_id ON time_entries(task_id);
CREATE INDEX IF NOT EXISTS idx_timeentries_user_id ON time_entries(user_id);
CREATE INDEX IF NOT EXISTS idx_timeentries_date ON time_entries(entry_date);
CREATE INDEX IF NOT EXISTS idx_timeentries_type ON time_entries(entry_type);
CREATE INDEX IF NOT EXISTS idx_timeentries_user_date ON time_entries(user_id, entry_date);

-- Create Risks table
CREATE TABLE IF NOT EXISTS risks (
    id UUID PRIMARY KEY,
    project_id UUID NOT NULL,
    owner_id UUID,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    status VARCHAR(50) NOT NULL,
    probability VARCHAR(50) NOT NULL,
    impact VARCHAR(50) NOT NULL,
    mitigation_plan TEXT,
    identified_date TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    CONSTRAINT fk_risks_project FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
    CONSTRAINT fk_risks_owner FOREIGN KEY (owner_id) REFERENCES users(id) ON DELETE SET NULL
);

-- Create indexes for Risks
CREATE INDEX IF NOT EXISTS idx_risks_project_id ON risks(project_id);
CREATE INDEX IF NOT EXISTS idx_risks_owner_id ON risks(owner_id);
CREATE INDEX IF NOT EXISTS idx_risks_status ON risks(status);
CREATE INDEX IF NOT EXISTS idx_risks_probability ON risks(probability);
CREATE INDEX IF NOT EXISTS idx_risks_impact ON risks(impact);

-- Create Issues table
CREATE TABLE IF NOT EXISTS issues (
    id UUID PRIMARY KEY,
    project_id UUID NOT NULL,
    reported_by_id UUID NOT NULL,
    assigned_to_id UUID,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    status VARCHAR(50) NOT NULL,
    severity VARCHAR(50) NOT NULL,
    reported_date TIMESTAMP NOT NULL,
    resolved_date TIMESTAMP,
    resolution TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    CONSTRAINT fk_issues_project FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
    CONSTRAINT fk_issues_reported_by FOREIGN KEY (reported_by_id) REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_issues_assigned_to FOREIGN KEY (assigned_to_id) REFERENCES users(id) ON DELETE SET NULL
);

-- Create indexes for Issues
CREATE INDEX IF NOT EXISTS idx_issues_project_id ON issues(project_id);
CREATE INDEX IF NOT EXISTS idx_issues_reported_by ON issues(reported_by_id);
CREATE INDEX IF NOT EXISTS idx_issues_assigned_to ON issues(assigned_to_id);
CREATE INDEX IF NOT EXISTS idx_issues_status ON issues(status);
CREATE INDEX IF NOT EXISTS idx_issues_severity ON issues(severity);

-- Create Documents table
CREATE TABLE IF NOT EXISTS documents (
    id UUID PRIMARY KEY,
    project_id UUID NOT NULL,
    uploaded_by_id UUID NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    file_size BIGINT NOT NULL,
    mime_type VARCHAR(100) NOT NULL,
    document_type VARCHAR(50) NOT NULL,
    version INTEGER NOT NULL DEFAULT 1,
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    CONSTRAINT fk_documents_project FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
    CONSTRAINT fk_documents_uploaded_by FOREIGN KEY (uploaded_by_id) REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT chk_document_size CHECK (file_size > 0)
);

-- Create indexes for Documents
CREATE INDEX IF NOT EXISTS idx_documents_project_id ON documents(project_id);
CREATE INDEX IF NOT EXISTS idx_documents_uploaded_by ON documents(uploaded_by_id);
CREATE INDEX IF NOT EXISTS idx_documents_type ON documents(document_type);

-- Create ResourceAllocations table
CREATE TABLE IF NOT EXISTS resource_allocations (
    id UUID PRIMARY KEY,
    project_id UUID NOT NULL,
    user_id UUID NOT NULL,
    start_date TIMESTAMP,
    end_date TIMESTAMP,
    allocation_percentage INTEGER NOT NULL,
    hourly_rate_amount DECIMAL(18,2),
    hourly_rate_currency VARCHAR(3),
    role VARCHAR(100),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    CONSTRAINT fk_allocations_project FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
    CONSTRAINT fk_allocations_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT chk_allocation_percentage CHECK (allocation_percentage >= 0 AND allocation_percentage <= 100),
    CONSTRAINT chk_allocation_dates CHECK (end_date IS NULL OR start_date IS NULL OR end_date >= start_date)
);

-- Create indexes for ResourceAllocations
CREATE INDEX IF NOT EXISTS idx_allocations_project_id ON resource_allocations(project_id);
CREATE INDEX IF NOT EXISTS idx_allocations_user_id ON resource_allocations(user_id);
CREATE INDEX IF NOT EXISTS idx_allocations_dates ON resource_allocations(start_date, end_date);

-- Create TaskComments table
CREATE TABLE IF NOT EXISTS task_comments (
    id UUID PRIMARY KEY,
    task_id UUID NOT NULL,
    user_id UUID NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    CONSTRAINT fk_comments_task FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE CASCADE,
    CONSTRAINT fk_comments_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT
);

-- Create indexes for TaskComments
CREATE INDEX IF NOT EXISTS idx_comments_task_id ON task_comments(task_id);
CREATE INDEX IF NOT EXISTS idx_comments_user_id ON task_comments(user_id);
CREATE INDEX IF NOT EXISTS idx_comments_created_at ON task_comments(created_at);

-- =====================================================
-- SEED DATA
-- =====================================================

-- Insert sample organizations
INSERT INTO organizations (id, name, description, street, city, state, country, postal_code, is_active, subscription_tier, subscription_start_date, subscription_end_date, created_at) VALUES
('11111111-1111-1111-1111-111111111111', 'Acme Corporation', 'Leading technology solutions provider', '123 Tech Street', 'San Francisco', 'California', 'USA', '94105', true, 'Enterprise', NOW() - INTERVAL '30 days', NOW() + INTERVAL '335 days', NOW() - INTERVAL '30 days'),
('22222222-2222-2222-2222-222222222222', 'TechStart Inc', 'Innovative startup company', '456 Startup Ave', 'Austin', 'Texas', 'USA', '78701', true, 'Professional', NOW() - INTERVAL '60 days', NOW() + INTERVAL '305 days', NOW() - INTERVAL '60 days'),
('33333333-3333-3333-3333-333333333333', 'Global Enterprises', 'International business solutions', '789 Business Blvd', 'New York', 'New York', 'USA', '10001', true, 'Basic', NOW() - INTERVAL '90 days', NOW() + INTERVAL '275 days', NOW() - INTERVAL '90 days')
ON CONFLICT (id) DO NOTHING;

-- Insert sample users (passwords are hashed for 'Password123!')
-- BCrypt hash: $2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5lW9P0zKJxZCG
INSERT INTO users (id, organization_id, first_name, last_name, email, password_hash, role, is_active, phone_number, failed_login_attempts, created_at) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111111', 'John', 'Admin', 'admin@acme.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5lW9P0zKJxZCG', 'SystemAdmin', true, '+1-555-0101', 0, NOW() - INTERVAL '30 days'),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '11111111-1111-1111-1111-111111111111', 'Sarah', 'Manager', 'sarah.manager@acme.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5lW9P0zKJxZCG', 'ProjectManager', true, '+1-555-0102', 0, NOW() - INTERVAL '28 days'),
('cccccccc-cccc-cccc-cccc-cccccccccccc', '11111111-1111-1111-1111-111111111111', 'Mike', 'Developer', 'mike.dev@acme.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5lW9P0zKJxZCG', 'Developer', true, '+1-555-0103', 0, NOW() - INTERVAL '25 days'),
('dddddddd-dddd-dddd-dddd-dddddddddddd', '22222222-2222-2222-2222-222222222222', 'Emily', 'Tech', 'emily@techstart.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5lW9P0zKJxZCG', 'ProjectManager', true, '+1-555-0104', 0, NOW() - INTERVAL '60 days'),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', '22222222-2222-2222-2222-222222222222', 'David', 'Designer', 'david@techstart.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5lW9P0zKJxZCG', 'Developer', true, '+1-555-0105', 0, NOW() - INTERVAL '58 days'),
('ffffffff-ffff-ffff-ffff-ffffffffffff', '33333333-3333-3333-3333-333333333333', 'Lisa', 'Global', 'lisa@global.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5lW9P0zKJxZCG', 'Viewer', true, '+1-555-0106', 0, NOW() - INTERVAL '90 days')
ON CONFLICT (email) DO NOTHING;

-- Insert sample projects
INSERT INTO projects (id, organization_id, project_manager_id, name, description, status, priority, start_date, end_date, budget_amount, budget_currency, actual_cost, progress_percentage, created_at) VALUES
('10000000-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'E-Commerce Platform', 'Build a scalable e-commerce platform with modern features', 'Active', 'High', NOW() - INTERVAL '60 days', NOW() + INTERVAL '120 days', 500000.00, 'USD', 125000.00, 35.50, NOW() - INTERVAL '60 days'),
('10000000-0000-0000-0000-000000000002', '11111111-1111-1111-1111-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Mobile App Development', 'Develop cross-platform mobile application', 'Planning', 'Medium', NOW() + INTERVAL '10 days', NOW() + INTERVAL '150 days', 300000.00, 'USD', 0.00, 0.00, NOW() - INTERVAL '5 days'),
('10000000-0000-0000-0000-000000000003', '22222222-2222-2222-2222-222222222222', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Website Redesign', 'Complete website redesign and rebranding', 'Active', 'High', NOW() - INTERVAL '30 days', NOW() + INTERVAL '60 days', 150000.00, 'USD', 45000.00, 55.00, NOW() - INTERVAL '30 days'),
('10000000-0000-0000-0000-000000000004', '22222222-2222-2222-2222-222222222222', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'API Integration', 'Integrate third-party APIs', 'Completed', 'Low', NOW() - INTERVAL '90 days', NOW() - INTERVAL '10 days', 50000.00, 'USD', 48000.00, 100.00, NOW() - INTERVAL '90 days')
ON CONFLICT (id) DO NOTHING;

-- Insert sample sprints
INSERT INTO sprints (id, project_id, name, goal, status, start_date, end_date, total_story_points, completed_story_points, created_at) VALUES
('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'Sprint 1 - Foundation', 'Setup project infrastructure and core features', 'Completed', NOW() - INTERVAL '60 days', NOW() - INTERVAL '46 days', 50, 50, NOW() - INTERVAL '60 days'),
('20000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'Sprint 2 - User Management', 'Implement user authentication and profiles', 'Completed', NOW() - INTERVAL '45 days', NOW() - INTERVAL '31 days', 45, 45, NOW() - INTERVAL '45 days'),
('20000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', 'Sprint 3 - Product Catalog', 'Build product catalog and search', 'Active', NOW() - INTERVAL '30 days', NOW() + INTERVAL '4 days', 55, 35, NOW() - INTERVAL '30 days'),
('20000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000003', 'Sprint 1 - Design', 'Complete UI/UX design', 'Completed', NOW() - INTERVAL '30 days', NOW() - INTERVAL '16 days', 30, 30, NOW() - INTERVAL '30 days'),
('20000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000003', 'Sprint 2 - Implementation', 'Implement new design', 'Active', NOW() - INTERVAL '15 days', NOW() + INTERVAL '5 days', 40, 22, NOW() - INTERVAL '15 days')
ON CONFLICT (id) DO NOTHING;

-- Insert sample tasks
INSERT INTO tasks (id, project_id, parent_task_id, assigned_to_id, title, description, status, priority, task_type, story_points, estimated_hours, actual_hours, due_date, is_blocked, created_at) VALUES
('30000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Setup Database Schema', 'Design and implement database schema for e-commerce', 'Done', 'High', 'Task', 8, 16.00, 14.50, NOW() - INTERVAL '50 days', false, NOW() - INTERVAL '58 days'),
('30000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Implement User Authentication', 'JWT-based authentication system', 'Done', 'High', 'Feature', 13, 24.00, 26.00, NOW() - INTERVAL '35 days', false, NOW() - INTERVAL '42 days'),
('30000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Build Product Catalog API', 'REST API for product management', 'InProgress', 'High', 'Feature', 21, 40.00, 28.50, NOW() + INTERVAL '5 days', false, NOW() - INTERVAL '28 days'),
('30000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000003', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Add Product Search', 'Implement search functionality', 'ToDo', 'Medium', 'Feature', 8, 16.00, 0.00, NOW() + INTERVAL '10 days', false, NOW() - INTERVAL '25 days'),
('30000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000003', NULL, 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Design Homepage', 'Create homepage mockups and prototypes', 'Done', 'High', 'Design', 5, 8.00, 8.00, NOW() - INTERVAL '20 days', false, NOW() - INTERVAL '28 days'),
('30000000-0000-0000-0000-000000000006', '10000000-0000-0000-0000-000000000003', NULL, 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Implement Responsive Design', 'Make website mobile-friendly', 'InProgress', 'High', 'Feature', 13, 24.00, 16.00, NOW() + INTERVAL '8 days', false, NOW() - INTERVAL '12 days'),
('30000000-0000-0000-0000-000000000007', '10000000-0000-0000-0000-000000000001', NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Write Unit Tests', 'Comprehensive unit test coverage', 'InProgress', 'Medium', 'Task', 5, 12.00, 6.00, NOW() + INTERVAL '3 days', false, NOW() - INTERVAL '15 days'),
('30000000-0000-0000-0000-000000000008', '10000000-0000-0000-0000-000000000002', NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Setup React Native Project', 'Initialize mobile app project', 'Backlog', 'High', 'Task', 3, 4.00, 0.00, NOW() + INTERVAL '15 days', false, NOW() - INTERVAL '3 days')
ON CONFLICT (id) DO NOTHING;

-- Insert sample time entries
INSERT INTO time_entries (id, task_id, user_id, hours, entry_date, description, is_billable, entry_type, created_at) VALUES
('40000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 8.00, NOW() - INTERVAL '55 days', 'Database design and planning', true, 'Development', NOW() - INTERVAL '55 days'),
('40000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000001', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 6.50, NOW() - INTERVAL '54 days', 'Schema implementation', true, 'Development', NOW() - INTERVAL '54 days'),
('40000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000002', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 8.00, NOW() - INTERVAL '40 days', 'JWT implementation', true, 'Development', NOW() - INTERVAL '40 days'),
('40000000-0000-0000-0000-000000000004', '30000000-0000-0000-0000-000000000002', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 7.50, NOW() - INTERVAL '39 days', 'Authentication testing', true, 'Testing', NOW() - INTERVAL '39 days'),
('40000000-0000-0000-0000-000000000005', '30000000-0000-0000-0000-000000000003', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 8.00, NOW() - INTERVAL '28 days', 'API endpoint development', true, 'Development', NOW() - INTERVAL '28 days'),
('40000000-0000-0000-0000-000000000006', '30000000-0000-0000-0000-000000000005', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 8.00, NOW() - INTERVAL '27 days', 'Homepage design', true, 'Design', NOW() - INTERVAL '27 days'),
('40000000-0000-0000-0000-000000000007', '30000000-0000-0000-0000-000000000006', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 8.00, NOW() - INTERVAL '10 days', 'Mobile responsive implementation', true, 'Development', NOW() - INTERVAL '10 days')
ON CONFLICT (id) DO NOTHING;

-- Insert sample risks
INSERT INTO risks (id, project_id, owner_id, title, description, status, probability, impact, mitigation_plan, identified_date, created_at) VALUES
('50000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Third-party API Dependency', 'Reliance on external payment gateway API', 'Active', 'Medium', 'High', 'Implement fallback payment providers and maintain service level agreements', NOW() - INTERVAL '45 days', NOW() - INTERVAL '45 days'),
('50000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Resource Availability', 'Key developer may leave the project', 'Mitigated', 'Low', 'Medium', 'Cross-train team members and document all processes', NOW() - INTERVAL '30 days', NOW() - INTERVAL '30 days'),
('50000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000003', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Browser Compatibility', 'Design may not work on older browsers', 'Active', 'Medium', 'Low', 'Test on multiple browsers and implement progressive enhancement', NOW() - INTERVAL '25 days', NOW() - INTERVAL '25 days')
ON CONFLICT (id) DO NOTHING;

-- Insert sample issues
INSERT INTO issues (id, project_id, reported_by_id, assigned_to_id, title, description, status, severity, reported_date, created_at) VALUES
('60000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Login page not responsive', 'Login page breaks on mobile devices', 'Resolved', 'Medium', NOW() - INTERVAL '20 days', NOW() - INTERVAL '20 days'),
('60000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Performance issue on product search', 'Search takes too long with large datasets', 'Open', 'High', NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days'),
('60000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000003', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Color contrast accessibility', 'Some text fails WCAG AA standards', 'InProgress', 'Low', NOW() - INTERVAL '8 days', NOW() - INTERVAL '8 days')
ON CONFLICT (id) DO NOTHING;

-- Insert sample documents
INSERT INTO documents (id, project_id, uploaded_by_id, file_name, file_path, file_size, mime_type, document_type, version, description, created_at) VALUES
('70000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'System Architecture.pdf', '/documents/ecommerce/architecture.pdf', 2048576, 'application/pdf', 'Technical', 1, 'System architecture diagram and documentation', NOW() - INTERVAL '55 days'),
('70000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'API Documentation.pdf', '/documents/ecommerce/api-docs.pdf', 1048576, 'application/pdf', 'Technical', 2, 'REST API documentation', NOW() - INTERVAL '40 days'),
('70000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000003', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Design Mockups.fig', '/documents/redesign/mockups.fig', 5242880, 'application/octet-stream', 'Design', 1, 'Figma design file with all mockups', NOW() - INTERVAL '28 days'),
('70000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000001', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Test Plan.docx', '/documents/ecommerce/test-plan.docx', 512000, 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', 'Planning', 1, 'Comprehensive test plan', NOW() - INTERVAL '15 days')
ON CONFLICT (id) DO NOTHING;

-- Insert sample resource allocations
INSERT INTO resource_allocations (id, project_id, user_id, start_date, end_date, allocation_percentage, hourly_rate_amount, hourly_rate_currency, role, created_at) VALUES
('80000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', NOW() - INTERVAL '60 days', NOW() + INTERVAL '120 days', 50, 150.00, 'USD', 'Project Manager', NOW() - INTERVAL '60 days'),
('80000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW() - INTERVAL '60 days', NOW() + INTERVAL '120 days', 100, 120.00, 'USD', 'Senior Developer', NOW() - INTERVAL '60 days'),
('80000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000003', 'dddddddd-dddd-dddd-dddd-dddddddddddd', NOW() - INTERVAL '30 days', NOW() + INTERVAL '60 days', 75, 140.00, 'USD', 'Project Manager', NOW() - INTERVAL '30 days'),
('80000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000003', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', NOW() - INTERVAL '30 days', NOW() + INTERVAL '60 days', 100, 110.00, 'USD', 'UI/UX Designer', NOW() - INTERVAL '30 days'),
('80000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000002', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', NOW() + INTERVAL '10 days', NOW() + INTERVAL '150 days', 25, 150.00, 'USD', 'Project Manager', NOW() - INTERVAL '5 days')
ON CONFLICT (id) DO NOTHING;

-- Insert sample task comments
INSERT INTO task_comments (id, task_id, user_id, content, created_at) VALUES
('90000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Great work on the database schema! Please also add indexes for performance.', NOW() - INTERVAL '52 days'),
('90000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000001', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Thanks! Indexes have been added as requested.', NOW() - INTERVAL '51 days'),
('90000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000003', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'How is the progress on the API? Need any support?', NOW() - INTERVAL '10 days'),
('90000000-0000-0000-0000-000000000004', '30000000-0000-0000-0000-000000000003', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Making good progress. Should be done by end of week.', NOW() - INTERVAL '9 days'),
('90000000-0000-0000-0000-000000000005', '30000000-0000-0000-0000-000000000006', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Please test on multiple devices before marking complete.', NOW() - INTERVAL '7 days'),
('90000000-0000-0000-0000-000000000006', '30000000-0000-0000-0000-000000000006', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Will do! Testing on iOS and Android.', NOW() - INTERVAL '6 days')
ON CONFLICT (id) DO NOTHING;

-- =====================================================
-- COMPLETION MESSAGE
-- =====================================================
\echo '=================================='
\echo 'Database initialization complete!'
\echo '=================================='
\echo 'Organizations: 3'
\echo 'Users: 6 (Password: Password123!)'
\echo 'Projects: 4'
\echo 'Sprints: 5'
\echo 'Tasks: 8'
\echo 'Time Entries: 7'
\echo 'Risks: 3'
\echo 'Issues: 3'
\echo 'Documents: 4'
\echo 'Resource Allocations: 5'
\echo 'Task Comments: 6'
\echo '=================================='
\echo 'You can now login with:'
\echo 'Email: admin@acme.com'
\echo 'Password: Password123!'
\echo '=================================='
