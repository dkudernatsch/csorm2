INSERT Into teacher (custom_name, name, firstname, bday)
VALUES (3500, 'abc1', 'def1', now()),
       (3500, 'abc2', 'def2', now()),
       (3500, 'abc3', 'def3', now()),
       (3500, 'abc4', 'def4', now());

INSERT Into class (name, fk_class_teacher)
VALUES ('BIF1', 1),
       ('BIF2', 2),
       ('BIF3', 3);

INSERT Into course(active, name, fk_course_teacher)
VALUES (true, 'SWE3', 1),
       (false, 'AI', 1),
       (true, 'SWE3', 2);

INSERT INTO student(id, name, firstname, bday, fk_student_class)
VALUES (1, 'ghi1', 'jkl1', now(), 1),
       (2, 'ghi2', 'jkl2', now(), 1),
       (3, 'ghi3', 'jkl3', now(), 2),
       (4, 'ghi4', 'jkl4', now(), 2),
       (5, 'ghi5', 'jkl5', now(), 3),
       (6, 'ghi6', 'jkl6', now(), 3);

