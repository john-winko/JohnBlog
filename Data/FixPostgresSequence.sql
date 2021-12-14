SELECT SETVAL('public."AspNetRoleClaims_Id_seq"', COALESCE(MAX("Id"), 1) ) FROM public."AspNetRoleClaims";
SELECT SETVAL('public."AspNetUserClaims_Id_seq"', COALESCE(MAX("Id"), 1) ) FROM public."AspNetUserClaims";
SELECT SETVAL('public."Blogs_Id_seq"', COALESCE(MAX("Id"), 1) ) FROM public."Blogs";
SELECT SETVAL('public."Comments_Id_seq"', COALESCE(MAX("Id"), 1) ) FROM public."Comments";
SELECT SETVAL('public."Posts_Id_seq"', COALESCE(MAX("Id"), 1) ) FROM public."Posts";
SELECT SETVAL('public."Tags_Id_seq"', COALESCE(MAX("Id"), 1) ) FROM public."Tags";
