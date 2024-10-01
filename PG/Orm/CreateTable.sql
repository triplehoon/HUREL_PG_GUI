-- Create the table for SessionInfo
CREATE TABLE SessionInfo (
    session_id VARCHAR(255) PRIMARY KEY,  -- session_id as primary key
    patient_number VARCHAR(30) NOT NULL,   -- patient_number (6-digit string)
    date DATE NOT NULL,                   -- date column
    is_running BOOLEAN NOT NULL           -- is_running (boolean)
);

-- Create the table for FpgaData
CREATE TABLE FpgaData (
    id SERIAL PRIMARY KEY,                -- id as primary key with auto increment
    channel INT NOT NULL,                 -- channel column (integer)
    timestamp_ns BIGINT NOT NULL,         -- timestamp_ns (big integer for nanoseconds)
    signal_value_mv DOUBLE PRECISION NOT NULL,  -- signal_value_mv (double precision)
    SessionInfo_id VARCHAR(255) REFERENCES SessionInfo(session_id)  -- foreign key linking to SessionInfo
);
