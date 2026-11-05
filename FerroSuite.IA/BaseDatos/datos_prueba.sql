/* =====================================================================
   FerroSuite — Datos de prueba
   Solo inserta si la base está vacía. El programa lo ejecuta al crear la
   base y cuando se usa "Restaurar datos de prueba".
   ===================================================================== */

USE FerroSuite;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Proveedor)
BEGIN
    INSERT INTO dbo.Proveedor (Nit, Nombre, Telefono, DiasEntrega, Cumplimiento) VALUES
    ('900123456-7', N'Cementos Andinos SAS', '601 555 0110', 3, 95),
    ('800987654-1', N'Aceros del Norte',     '601 555 0221', 5, 88),
    ('901222333-9', N'Eléctricos JR',        '601 555 0330', 2, 97);

    INSERT INTO dbo.Producto (Sku, Nombre, Categoria, Unidad, StockActual, StockMinimo, PrecioCompra, NitProveedor) VALUES
    ('FER-0245', N'Cemento gris 50 kg',          N'Construcción', N'Bulto',     120,  30, 29500, '900123456-7'),
    ('FER-0110', N'Varilla corrugada 1/2"',      N'Construcción', N'Varilla',    18,  20, 21800, '800987654-1'),
    ('FER-0087', N'Cable eléctrico THHN 12',     N'Eléctricos',   N'Metro',     340,  50,  2100, '901222333-9'),
    ('FER-0033', N'Tornillo autorroscante 8x1',  N'Tornillería',  N'Unidad',    900, 200,    90, '800987654-1'),
    ('FER-0199', N'Pintura blanca tipo 1',       N'Pinturas',     N'Galón',      12,  15, 48000, '900123456-7'),
    ('FER-0301', N'Tubo PVC presión 1/2"',       N'Plomería',     N'Unidad',     26,  15,  9800, '900123456-7'),
    ('FER-0302', N'Codo PVC 1/2"',               N'Plomería',     N'Unidad',     40,  30,   650, '900123456-7'),
    ('FER-0303', N'Soldadura PVC 1/4 gal',       N'Plomería',     N'Unidad',      4,   6, 23500, '900123456-7'),
    ('FER-0410', N'Alambre negro calibre 18',    N'Construcción', N'Kilogramo',  22.5, 10,  7600, '800987654-1'),
    ('FER-0520', N'Interruptor sencillo',        N'Eléctricos',   N'Unidad',     35,  12,  5200, '901222333-9'),
    ('FER-0611', N'Martillo de uña 16 oz',       N'Herramientas', N'Unidad',      9,   4, 26000, '800987654-1'),
    ('FER-0612', N'Disco de corte metal 4 1/2"', N'Herramientas', N'Unidad',      0,  10,  4300, '800987654-1');

    /* Ventas del último mes (cada 3 días), para calcular cuánto se vende por día */
    INSERT INTO dbo.MovimientoInventario (Sku, Tipo, Cantidad, Motivo, Usuario, Fecha)
    SELECT v.Sku, 'Salida', v.PorDia * 3, N'Venta', N'Datos de prueba',
           DATEADD(DAY, -d.Dia, CAST(CAST(SYSDATETIME() AS DATE) AS DATETIME2(0)))
    FROM (VALUES ('FER-0245', 6), ('FER-0110', 3), ('FER-0087', 14), ('FER-0033', 45),
                 ('FER-0199', 1), ('FER-0301', 3), ('FER-0302', 2), ('FER-0303', 0.5),
                 ('FER-0410', 1.5), ('FER-0520', 1), ('FER-0612', 2)) AS v (Sku, PorDia)
    CROSS JOIN (VALUES (28), (25), (22), (19), (16), (13), (10), (7), (4), (1)) AS d (Dia);

    /* Una orden aprobada sin pagar (su pago vence pronto) y una por aprobar */
    INSERT INTO dbo.OrdenCompra (Sku, NitProveedor, Cantidad, Estado, FechaCreacion, FechaAprobacion, Pagada) VALUES
    ('FER-0110', '800987654-1', 100, N'Aprobada',                DATEADD(DAY, -14, SYSDATETIME()), DATEADD(DAY, -12, SYSDATETIME()), 0),
    ('FER-0612', '800987654-1',  50, N'Pendiente de aprobación', DATEADD(DAY, -3,  SYSDATETIME()), NULL, 0);
END
GO
