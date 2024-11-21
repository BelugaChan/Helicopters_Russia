-- Создание базы данных, если она не существует (не используйте в DO-блоке)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = 'Heli_Russia') THEN
        CREATE DATABASE Heli_Russia;
    END IF;
END $$;

-- Создание роли, если она не существует
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'admin') THEN
        CREATE ROLE admin LOGIN PASSWORD 'admin';
    END IF;
END $$;


\connect Heli_Russia;


CREATE TABLE IF NOT EXISTS standarts (
    id UUID PRIMARY KEY,
    code TEXT NOT NULL,
    name TEXT NOT NULL
);
