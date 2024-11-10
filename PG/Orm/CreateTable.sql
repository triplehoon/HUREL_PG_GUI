-- Create SessionInfo table
CREATE TABLE SessionInfo (
    session_id VARCHAR(50) PRIMARY KEY,  -- assuming a maximum length of 50 characters for session_id
    patient_number VARCHAR(6),
    date DATE,
    is_running BOOLEAN,
    path_calibration_file VARCHAR(255),
    path_pld_file VARCHAR(255),
    path_3d_pld_file VARCHAR(255)
);

-- Create SessionLogSpots table
CREATE TABLE SessionLogSpots (
    id INT PRIMARY KEY,
    session_id VARCHAR(50),
    spot_sequence_number INT,
    layer_index INT,
    is_tunning BOOLEAN DEFAULT FALSE,
    part_index INT,
    resume_index INT,
    position_x DOUBLE PRECISION,
    position_y DOUBLE PRECISION,
    FOREIGN KEY (session_id) REFERENCES SessionInfo(session_id)
);

-- Create SessionAggSpots table
CREATE TABLE SessionAggSpots (
    id INT PRIMARY KEY,
    session_id VARCHAR(50),
    log_spot_id INT,
    plan_layer_index INT,
    plan_proton_beam_energy DOUBLE PRECISION,
    plan_position_x DOUBLE PRECISION,
    plan_position_y DOUBLE PRECISION,
    plan_position_z DOUBLE PRECISION,
    plan_monitor_unit DOUBLE PRECISION,
    range_difference DOUBLE PRECISION,
    agreegate_monitor_unit DOUBLE PRECISION,
    true_range_80 DOUBLE PRECISION,
    FOREIGN KEY (session_id) REFERENCES SessionInfo(session_id),
    FOREIGN KEY (log_spot_id) REFERENCES SessionLogSpots(id)
);
