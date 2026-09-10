CREATE TABLE premises (
    premises_id SERIAL PRIMARY KEY,
    premises_type VARCHAR(20) NOT NULL CHECK (premises_type IN ('житлове', 'нежитлове')),
    purpose VARCHAR(30) NOT NULL CHECK (purpose IN ('оренда', 'продаж', 'оренда/продаж')),
    street VARCHAR(150) NOT NULL,
    building_number VARCHAR(20) NOT NULL,
    apartment_number INT,
    entrance_number INT,
    total_area NUMERIC(7, 2) NOT NULL CHECK (total_area > 0),
    living_area NUMERIC(7, 2) CHECK (living_area IS NULL OR living_area <= total_area),
    rooms_count INT NOT NULL DEFAULT 1 CHECK (rooms_count > 0),
    has_kitchen BOOLEAN NOT NULL DEFAULT TRUE,
    bathrooms_count INT NOT NULL DEFAULT 1 CHECK (bathrooms_count >= 0),
    price NUMERIC(12, 2) NOT NULL CHECK (price > 0),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);



INSERT INTO premises (
    premises_type, 
    purpose, 
    street, 
    building_number, 
    apartment_number, 
    entrance_number, 
    total_area, 
    living_area, 
    rooms_count, 
    has_kitchen, 
    bathrooms_count, 
    price
) VALUES 
('житлове', 'продаж', 'вул. Городоцька', '124', 45, 2, 65.50, 42.00, 2, TRUE, 1, 75000.00),
('житлове', 'оренда', 'просп. Свободи', '15', 8, 1, 45.00, 28.00, 1, TRUE, 1, 16000.00),
('житлове', 'оренда/продаж', 'вул. Стрийська', '85Б', 112, 4, 98.40, 64.20, 3, TRUE, 2, 120000.00),
('нежитлове', 'оренда', 'вул. Наукова', '7А', NULL, NULL, 135.00, NULL, 4, FALSE, 2, 42000.00),
('нежитлове', 'продаж', 'вул. Зелена', '204', NULL, NULL, 210.50, NULL, 2, FALSE, 1, 185000.00),
('житлове', 'оренда', 'вул. Франка', '32', 3, 1, 32.00, 22.00, 1, TRUE, 1, 12500.00),
('житлове', 'продаж', 'вул. Шевченка', '310', NULL, NULL, 175.80, 110.50, 5, TRUE, 2, 165000.00),
('нежитлове', 'оренда', 'вул. Промислова', '12', NULL, NULL, 450.00, NULL, 1, FALSE, 0, 55000.00),
('житлове', 'продаж', 'вул. Личаківська', '68', 19, 1, 142.30, 95.00, 4, TRUE, 3, 195000.00),
('нежитлове', 'оренда', 'вул. Дорошенка', '9', NULL, NULL, 58.00, NULL, 2, TRUE, 1, 28000.00);


select street, building_number, price, total_area, ROUND(price / total_area, 2) AS price_per_sq_meter
FROM premises;


SELECT premises_id, premises_type, rooms_count, total_area, price
FROM premises
WHERE premises_type = 'житлове'
  AND rooms_count <> 1
  AND total_area BETWEEN 50.00 AND 150.00;


SELECT premises_id, premises_type, purpose, street ,price, has_kitchen
FROM premises
WHERE NOT (premises_type = 'нежитлове')
  AND has_kitchen = TRUE
  AND (purpose = 'оренда' AND price <= 20000.00);


SELECT premises_id, purpose, price, street, building_number
FROM premises
ORDER BY purpose ASC, price DESC;


SELECT premises_id, street, building_number
FROM premises
WHERE street LIKE 'вул.%а%'
  AND building_number LIKE '__%';

SELECT premises_id, premises_type, street, building_number, apartment_number, living_area
FROM premises
WHERE apartment_number IS null;

