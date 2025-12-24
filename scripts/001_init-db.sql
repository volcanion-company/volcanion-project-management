-- Tạo database
CREATE DATABASE volcanionpm 
ENCODING 'UTF8'
LC_COLLATE 'en_US.UTF-8'
LC_CTYPE 'en_US.UTF-8'
TEMPLATE template0;

-- Tạo user
CREATE USER pmuser WITH PASSWORD '52r4LHYqnwFT37OMRmh4tOP2F';

-- Gán owner cho database
ALTER DATABASE volcanionpm OWNER TO pmuser;

-- Kết nối và thiết lập quyền trong database volcanionpm
\connect volcanionpm

-- Thiết lập schema
ALTER SCHEMA public OWNER TO pmuser;
GRANT ALL ON SCHEMA public TO pmuser;

-- Thiết lập default privileges
ALTER DEFAULT PRIVILEGES FOR USER pmuser IN SCHEMA public 
GRANT ALL ON TABLES TO pmuser;

ALTER DEFAULT PRIVILEGES FOR USER pmuser IN SCHEMA public 
GRANT ALL ON SEQUENCES TO pmuser;
ALTER DEFAULT PRIVILEGES FOR USER pmuser IN SCHEMA public 
GRANT ALL ON FUNCTIONS TO pmuser;