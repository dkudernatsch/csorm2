create table Course(
    Id bigint primary key,
    Name Text,
    Room text
);

create table Grade (
    Id bigint primary key,
    GradeValue INTEGER,
    FK_Grade_Student bigint
);

create table Student(
    Id bigserial primary key,
    Name Text
);

alter table Grade add Constraint FK_Constraint_Grade_Student FOREIGN KEY(FK_Grade_Student) REFERENCES Student(Id);

create table relcoursestudent(
    id bigserial primary key,
    fk_course bigint REFERENCES Course(Id),
    fk_student bigint REFERENCES Student(Id)
);

insert into Student(Name) VALUES 
('Daniel Kudernatsch'),
('Viktor Leher');


INSERT INTO public.course (id, name, room) VALUES (1, 'SWE3', 'F2.03');
INSERT INTO public.course (id, name, room) VALUES (2, 'FUS', 'A6.10');

INSERT INTO public.grade (id, gradevalue, fk_grade_student) VALUES (1, 1, 1);
INSERT INTO public.grade (id, gradevalue, fk_grade_student) VALUES (2, 2, 1);
INSERT INTO public.grade (id, gradevalue, fk_grade_student) VALUES (3, 3, 1);
INSERT INTO public.grade (id, gradevalue, fk_grade_student) VALUES (4, 2, 2);
INSERT INTO public.grade (id, gradevalue, fk_grade_student) VALUES (5, 3, 2);


INSERT INTO public.relcoursestudent (fk_course, fk_student) VALUES ( 1, 1);
INSERT INTO public.relcoursestudent (fk_course, fk_student) VALUES ( 2, 1);
INSERT INTO public.relcoursestudent (fk_course, fk_student) VALUES ( 2, 2);