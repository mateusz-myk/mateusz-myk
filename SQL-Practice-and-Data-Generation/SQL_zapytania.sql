SELECT MIN(actor_id) AS min_walue FROM actor

SELECT MAX(actor_id) AS min_walue FROM actor

SELECT COUNT(actor_id) FROM actor WHERE first_name = 'BOB';

SELECT count(actor_id)
FROM actor
WHERE first_name LIKE 'B%';

SELECT SUM(address2)
FROM address

SELECT AVG(actor_id) from actor


SELECT title
FROM film
ORDER BY last_update ASC
LIMIT 1;  

SELECT title
FROM film
ORDER BY last_update ASC
LIMIT 1;  

SELECT title
FROM film
ORDER BY title
desc LIMIT 1 OFFSET 9;

SELECT first_name, last_name
FROM actor
WHERE actor_id IN (
    SELECT actor_id
    FROM film_actor
    WHERE film_id IN (
        SELECT film_id
        FROM film
        WHERE description LIKE '%dog%'
        AND title LIKE '%wolves%'
    )
)

9

SELECT title
FROM film
WHERE special_features NOT LIKE '%Deleted Scenes%'
ORDER BY length DESC
LIMIT 1;

10

SELECT COUNT(*), store_id
From sakila.inventory
WHERE film_id = 182
GROUP BY store_id;

11

SELECT first_name, last_name
FROM sakila.customer
WHERE customer_id IN (
    SELECT customer_id
    FROM sakila.rental
    WHERE return_date IS NOT NULL
    AND inventory_id IN (
    SELECT inventory_id
    FROM sakila.inventory
    WHERE film_id = 182
    )
)

12

SELECT first_name, last_name, active
from customer
WHERE customer_id in (
    SELECT customer_id
    FROM sakila.rental
    WHERE return_date IS NOT NULL
    AND inventory_id IN (
    SELECT inventory_id
    FROM sakila.inventory
    WHERE film_id = 182
    )
)

13

SELECT phone from 

14

SELECT actor.first_name, actor.last_name, (
    SELECT COUNT(film_actor.film_id)
    FROM film_actor
    WHERE film_actor.actor_id = actor.actor_id
) AS ilosc_filmow
FROM actor
WHERE actor.first_name = 'Penelope'
ORDER BY ilosc_filmow DESC
LIMIT 1;


SELECT actor_id, COUNT(film_id) AS liczba_filmow
FROM sakila.film_actor
WHERE actor_id IN (
    SELECT actor_id FROM sakila.actor
    WHERE first_name = "Penelope"
)
GROUP BY actor_id ORDER BY count(film_id) desc LIMIT 1;

15

SELECT first_name, last_name FROM sakila.staff
WHERE staff_id = (
    SELECT staff_id FROM sakila.payment
    GROUP BY staff_id
    ORDER BY SUM(amount)
    DESC
    LIMIT 1
)

16

SELECT staff_id, COUNT(payment_id)
    from sakila.payment
    WHERE payment_date >= 20050701
    AND payment_date <= 20050831
    GROUP BY staff_id DESC
    LIMIT 1


17

SELECT count(email)
FROM sakila.customer
WHERE email NOT LIKE "%@sakilacustomer.org"

18

SELECT COUNT(*), store_id
FROM sakila.customer
WHERE active = 1
GROUP BY store_id;

19

SELECT staff_id, COUNT(rental_id) FROM sakila.rental
    WHERE return_date IS NULL
    GROUP BY staff_id

20

SELECT phone
FROM address
WHERE address_id = (
    SELECT address_id
    FROM customer
    WHERE customer_id = (
        SELECT customer_id
        FROM rental
        WHERE return_date is NULL
        ORDER BY rental_date ASC
        LIMIT 1
        )
)

3.1

SELECT COUNT (*)
from employees.dept_emp
WHERE CURRENT_DATE < to_date;

2




3
