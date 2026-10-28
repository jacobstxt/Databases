import psycopg2


COLUMN = "full_name"

conn = psycopg2.connect(
    host="localhost",
    port=5433,
    dbname="coursedb",
    user="reader_user",
    password="Reader123!",
)
cur = conn.cursor()

def find_seller_safe(name):

    query = "SELECT * FROM sellers WHERE " + COLUMN + " = %s"
    print("Запит, що виконується:")
    print(query, "| параметр:", repr(name))
    cur.execute(query, (name,))
    return cur.fetchall()

def find_seller_vulnerable(name):
    query = "SELECT * FROM sellers WHERE " + COLUMN + " = '" + name + "'"
    print("Запит, що виконується:")
    print(query)
    cur.execute(query)
    return cur.fetchall()


print("=== 1. Нормальний ввід ===")
rows = find_seller_vulnerable("Симоненко Віктор Павлович")
print("Знайдено рядків:", len(rows))

print("\n=== 2. SQL-ін'єкція ===")
rows = find_seller_vulnerable("' OR '1'='1")
print("Знайдено рядків:", len(rows))
for r in rows[:5]:
    print(r)

print("\n=== 3. Параметризований запит: нормальний ввід ===")
rows = find_seller_safe("Симоненко Віктор Павлович")
print("Знайдено рядків:", len(rows))

print("\n=== 4. Параметризований запит: спроба ін'єкції ===")
rows = find_seller_safe("' OR '1'='1")
print("Знайдено рядків:", len(rows))

cur.close()
conn.close()