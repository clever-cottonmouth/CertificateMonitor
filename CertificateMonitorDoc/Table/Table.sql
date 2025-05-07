CREATE DATABASE certificate_monitor;

USE certificate_monitor;

CREATE TABLE certificate_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    timestamp DATETIME NOT NULL,
    process_name VARCHAR(255),
    window_title VARCHAR(255),
    certificate_subject TEXT,
    thumbprint VARCHAR(255)
);