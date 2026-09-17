CREATE TABLE buyers (
    buyer_id SERIAL PRIMARY KEY,
    full_name VARCHAR(150) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(100),
    bank_account VARCHAR(50) NOT NULL,
    bank_name VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE sellers (
    seller_id SERIAL PRIMARY KEY,
    full_name VARCHAR(150) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(100),
    bank_account VARCHAR(50) NOT NULL,
    bank_name VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);



CREATE TABLE deals (
    deal_id SERIAL PRIMARY KEY,
    premises_id INT NOT NULL REFERENCES premises(premises_id) ON DELETE RESTRICT ON UPDATE CASCADE,
    seller_id INT NOT NULL REFERENCES sellers(seller_id) ON DELETE RESTRICT ON UPDATE CASCADE, 
    buyer_id INT NOT NULL REFERENCES buyers(buyer_id) ON DELETE RESTRICT ON UPDATE CASCADE,
    deal_type VARCHAR(20) NOT NULL CHECK (deal_type IN ('продаж', 'оренда')),
    deal_cost NUMERIC(12, 2) NOT NULL CHECK (deal_cost > 0),
    deal_date DATE NOT NULL DEFAULT CURRENT_DATE,
    rental_term_months INT CHECK (
        (deal_type = 'оренда' AND rental_term_months > 0) OR 
        (deal_type = 'продаж' AND rental_term_months IS NULL)
    ),
    rental_conditions TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO buyers (full_name, phone, email, bank_account, bank_name) VALUES
('Коваленко Петро Іванович', '+380501112233', 'kovalenko@gmail.com', 'UA2638000000001234567890123', 'PrivatBank'),
('Мельник Олена Сергіївна', '+380672223344', 'melnyk.olena@ukr.net', 'UA3235100000002345678901234', 'Monobank'),
('Шевченко Андрій Васильович', '+380633334455', 'shevchenko.a@gmail.com', 'UA1430034600000345678901235', 'PUMB'),
('Бойко Наталія Павлівна', '+380954445566', 'boyko.n@gmail.com', 'UA883223130000045678901236', 'PrivatBank'),
('Ковальчук Максим Олегович', '+380975556677', 'kovalchuk.m@ukr.net', 'UA55305299000005678901237', 'Sense Bank'),
('Ткаченко Ірина Володимирівна', '+380506667788', 'tkachenko.i@gmail.com', 'UA993510000000678901238', 'Monobank'),
('Олійник Олег Михайлович', '+380677778899', 'oliinyk.o@gmail.com', 'UA123003460000078901239', 'PUMB'),
('Павленко Світлана Ігорівна', '+380638889900', 'pavlenko.s@ukr.net', 'UA44322313000008901240', 'PrivatBank'),
('Бондар Дмитро Петрович', '+380959990011', 'bondar.d@gmail.com', 'UA77305299000009012341', 'A-Bank'),
('Гнатюк Юлія Андріївна', '+380970001122', 'hnatiuk.y@ukr.net', 'UA21351000000010123452', 'Monobank');


INSERT INTO sellers (full_name, phone, email, bank_account, bank_name) VALUES
('Симоненко Віктор Павлович', '+380671234567', 'symonenko.v@gmail.com', 'UA45322313000001112233445', 'PrivatBank'),
('Кравченко Марія Іванівна', '+380502345678', 'kravchenko.m@ukr.net', 'UA89351000000022233445566', 'Monobank'),
('Лисенко Василь Петрович', '+380633456789', 'lysenko.v@gmail.com', 'UA11300346000003334455667', 'PUMB'),
('Романенко Тетяна Олегівна', '+380954567890', 'romanenko.t@gmail.com', 'UA67322313000004445566778', 'PrivatBank'),
('Коваль Сергій Володимирович', '+380975678901', 'koval.s@ukr.net', 'UA90305299000005556677889', 'Sense Bank'),
('Мороз Оксана Анатоліївна', '+380506789012', 'moroz.o@gmail.com', 'UA33351000000066677889900', 'Monobank'),
('Василенко Іван Григорович', '+380677890123', 'vasylenko.i@gmail.com', 'UA55300346000007778899001', 'PUMB'),
('Дмитренко Наталія Вікторівна', '+380638901234', 'dmytrenko.n@ukr.net', 'UA22322313000008889900112', 'PrivatBank'),
('Савченко Андрій Миколайович', '+380959012345', 'savchenko.a@gmail.com', 'UA88305299000009990011223', 'A-Bank'),
('Гриценко Людмила Павлівна', '+380970123456', 'hrytsenko.l@ukr.net', 'UA74351000000010101122334', 'Monobank');


INSERT INTO deals (premises_id, seller_id, buyer_id, deal_type, deal_cost, deal_date, rental_term_months, rental_conditions) VALUES
(4, 7, 2, 'продаж', 98000.00, '2026-09-10', NULL, NULL),
(1, 9, 5, 'оренда', 12000.00, '2026-09-11', 12, 'Оплата щомісяця до 5-го числа, без тварин'),
(8, 3, 10, 'продаж', 125000.00, '2026-09-12', NULL, NULL),
(2, 6, 1, 'оренда', 15000.00, '2026-09-13', 6, 'Передоплата за 2 місяці, можливий торг'),
(10, 2, 8, 'продаж', 75000.00, '2026-09-14', NULL, NULL),
(5, 10, 4, 'оренда', 10000.00, '2026-09-14', 12, 'Комунальні послуги сплачуються окремо'),
(3, 4, 7, 'продаж', 150000.00, '2026-09-15', NULL, NULL),
(7, 1, 3, 'оренда', 18000.00, '2026-09-15', 3, 'Короткострокова оренда для офісу'),
(6, 8, 9, 'продаж', 82000.00, '2026-09-16', NULL, NULL),
(9, 5, 6, 'оренда', 14000.00, '2026-09-16', 12, 'Довгостроковий контракт з можливістю пролонгації');

CREATE OR REPLACE VIEW v_deal_details AS
SELECT 
    d.deal_id,
    d.deal_type,
    d.deal_cost,
    d.deal_date,
    p.street,
    p.building_number,
    b.full_name AS buyer_name,
    b.phone AS buyer_phone,
    b.bank_account as buyer_bank_account,
    b.bank_name as buyer_bank_name
FROM deals d
JOIN premises p ON d.premises_id = p.premises_id
JOIN buyers b ON d.buyer_id = b.buyer_id;



SELECT * FROM v_deal_details WHERE deal_type = 'продаж';

DELETE FROM premises WHERE premises_id = 1;