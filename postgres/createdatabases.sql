CREATE USER lab_user WITH ENCRYPTED PASSWORD 'lab_pass';
CREATE DATABASE lab_netmicro_payments OWNER lab_user;
CREATE DATABASE lab_netmicro_jobs OWNER lab_user;
CREATE DATABASE lab_netmicro_policy OWNER lab_user;
CREATE DATABASE lab_netmicro_pricing OWNER lab_user;
