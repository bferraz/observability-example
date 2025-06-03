-- SELECT * FROM "Vouchers"

INSERT INTO "Vouchers" ("Id", "Code", "Description", "Discount", "ExpiryDate")
VALUES ('35bb3de5-b542-44cf-a6ac-84e3d291ddaa', '205897', 'Voucher 10%', 10, '2025-08-01 23:59:59');

-- SELECT * FROM "Products"

UPDATE "Products" SET "QtdStock" = 1 WHERE "Id" = '3ef6f085-d567-4ba4-9368-e320a2b923a7';
UPDATE "Products" SET "QtdStock" = 1 WHERE "Id" = '3fa85f64-5717-4562-b3fc-2c963f66afa6';
UPDATE "Products" SET "QtdStock" = 2 WHERE "Id" = 'eef8e519-7b44-49fc-bf79-30729ce1fa1e';