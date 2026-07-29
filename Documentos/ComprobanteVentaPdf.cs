using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFarmaciaWeb.Models;

namespace SistemaFarmaciaWeb.Documentos
{
    public class ComprobanteVentaPdf : IDocument
    {
        private readonly Venta _venta;

        public ComprobanteVentaPdf(Venta venta)
        {
            _venta = venta;
        }

        public DocumentMetadata GetMetadata()
        {
            return new DocumentMetadata
            {
                Title = $"Comprobante de venta #{_venta.IdVenta}",
                Author = "FarmaVentas",
                Subject = "Comprobante de venta",
                Keywords = "venta, farmacia, comprobante"
            };
        }

        public DocumentSettings GetSettings()
        {
            return DocumentSettings.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.PageColor(Colors.White);

                page.DefaultTextStyle(texto =>
                    texto.FontSize(10)
                );

                page.Header()
                    .Element(ConstruirEncabezado);

                page.Content()
                    .PaddingVertical(20)
                    .Element(ConstruirContenido);

                page.Footer()
                    .AlignCenter()
                    .Text(texto =>
                    {
                        texto.Span("Página ");
                        texto.CurrentPageNumber();
                        texto.Span(" de ");
                        texto.TotalPages();
                    });
            });
        }

        private void ConstruirEncabezado(IContainer container)
        {
            container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingBottom(15)
                .Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item()
                            .Text("FarmaVentas")
                            .FontSize(22)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        column.Item()
                            .Text("Sistema web de ventas de farmacia")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    row.RelativeItem().AlignRight().Column(column =>
                    {
                        column.Item()
                            .AlignRight()
                            .Text($"COMPROBANTE N.º {_venta.IdVenta}")
                            .FontSize(14)
                            .Bold();

                        column.Item()
                            .AlignRight()
                            .Text(
                                _venta.FechaVenta.ToString(
                                    "dd/MM/yyyy HH:mm"
                                )
                            )
                            .FontColor(Colors.Grey.Darken1);
                    });
                });
        }

        private void ConstruirContenido(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(18);

                column.Item()
                    .Element(ConstruirInformacionVenta);

                column.Item()
                    .Element(ConstruirTablaProductos);

                column.Item()
                    .AlignRight()
                    .Element(ConstruirTotales);

                column.Item()
                    .PaddingTop(15)
                    .AlignCenter()
                    .Text("Gracias por su compra")
                    .FontSize(12)
                    .SemiBold()
                    .FontColor(Colors.Blue.Darken2);
            });
        }

        private void ConstruirInformacionVenta(IContainer container)
        {
            container
                .Background(Colors.Grey.Lighten4)
                .Padding(15)
                .Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item()
                            .Text("Información de la venta")
                            .Bold()
                            .FontSize(12);

                        column.Item().PaddingTop(5).Text(texto =>
                        {
                            texto.Span("Número de venta: ").SemiBold();
                            texto.Span($"#{_venta.IdVenta}");
                        });

                        column.Item().Text(texto =>
                        {
                            texto.Span("Fecha: ").SemiBold();

                            texto.Span(
                                _venta.FechaVenta.ToString(
                                    "dd/MM/yyyy HH:mm"
                                )
                            );
                        });
                    });

                    row.RelativeItem().Column(column =>
                    {
                        column.Item()
                            .Text("Usuario")
                            .Bold()
                            .FontSize(12);

                        column.Item().PaddingTop(5).Text(
                            _venta.IdUsuarioNavigation?.Nombre
                            ?? "Usuario no disponible"
                        );
                    });
                });
        }

        private void ConstruirTablaProductos(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(85);
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Element(EstiloEncabezado)
                        .AlignCenter()
                        .Text("N.º");

                    header.Cell()
                        .Element(EstiloEncabezado)
                        .Text("Producto");

                    header.Cell()
                        .Element(EstiloEncabezado)
                        .AlignCenter()
                        .Text("Cantidad");

                    header.Cell()
                        .Element(EstiloEncabezado)
                        .AlignRight()
                        .Text("Precio");

                    header.Cell()
                        .Element(EstiloEncabezado)
                        .AlignRight()
                        .Text("Subtotal");
                });

                int numero = 1;

                foreach (DetalleVenta detalle in _venta.DetalleVenta)
                {
                    decimal subtotal =
                        detalle.Cantidad * detalle.PrecioUnitario;

                    table.Cell()
                        .Element(EstiloCelda)
                        .AlignCenter()
                        .Text(numero.ToString());

                    table.Cell()
                        .Element(EstiloCelda)
                        .Text(
                            detalle.IdProductoNavigation?.Nombre
                            ?? "Producto no disponible"
                        );

                    table.Cell()
                        .Element(EstiloCelda)
                        .AlignCenter()
                        .Text(detalle.Cantidad.ToString());

                    table.Cell()
                        .Element(EstiloCelda)
                        .AlignRight()
                        .Text(
                            $"Bs. {detalle.PrecioUnitario:N2}"
                        );

                    table.Cell()
                        .Element(EstiloCelda)
                        .AlignRight()
                        .Text($"Bs. {subtotal:N2}");

                    numero++;
                }
            });
        }

        private void ConstruirTotales(IContainer container)
        {
            container
                .Width(260)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(15)
                .Column(column =>
                {
                    column.Spacing(8);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Total:");

                        row.ConstantItem(110)
                            .AlignRight()
                            .Text($"Bs. {_venta.Total:N2}")
                            .Bold();
                    });

                    column.Item().Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Monto pagado:");

                        row.ConstantItem(110)
                            .AlignRight()
                            .Text($"Bs. {_venta.MontoPagado:N2}");
                    });

                    column.Item()
                        .BorderTop(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .PaddingTop(8)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text("Cambio:")
                                .Bold();

                            row.ConstantItem(110)
                                .AlignRight()
                                .Text(
                                    $"Bs. {(_venta.Cambio ?? 0):N2}"
                                )
                                .Bold()
                                .FontColor(Colors.Green.Darken2);
                        });
                });
        }

        private static IContainer EstiloEncabezado(
            IContainer container)
        {
            return container
                .Background(Colors.Blue.Darken2)
                .PaddingVertical(8)
                .PaddingHorizontal(6)
                .DefaultTextStyle(texto =>
                    texto
                        .FontColor(Colors.White)
                        .SemiBold()
                        .FontSize(9)
                );
        }

        private static IContainer EstiloCelda(
            IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .PaddingVertical(8)
                .PaddingHorizontal(6);
        }
    }
}