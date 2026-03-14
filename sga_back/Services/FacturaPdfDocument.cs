using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using sga_back.DTOs;
using System.Globalization;

namespace sga_back.Services;

public class FacturaPdfDocument
{
    private readonly FacturaPdfDto _model;

    public FacturaPdfDocument(FacturaPdfDto model)
    {
        _model = model;
    }

    public byte[] GeneratePdf()
    {
        using MemoryStream stream = new();
        using PdfWriter writer = new(stream);
        using PdfDocument pdf = new(writer);
        using Document document = new(pdf, PageSize.A4);

        document.SetMargins(18, 18, 18, 18);

        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        document.SetFont(font).SetFontSize(10);

        document.Add(new Paragraph("Factura")
            .SetFont(fontBold)
            .SetFontSize(14)
            .SetMarginBottom(12));

        document.Add(CrearPanelEncabezado(fontBold));
        document.Add(CrearPanelCliente(fontBold));
        document.Add(CrearPanelDetalles(fontBold));

        if (EsAnulada())
        {
            document.Add(CrearPanelAnulacion(fontBold));
        }

        document.Flush();

        if (EsAnulada())
        {
            AgregarMarcaAgua(pdf, fontBold);
        }

        document.Close();
        return stream.ToArray();
    }

    private Table CrearPanelEncabezado(PdfFont fontBold)
    {
        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }))
            .UseAllAvailableWidth()
            .SetBorder(new SolidBorder(new DeviceRgb(217, 217, 217), 1))
            .SetMarginBottom(12);

        Cell leftCell = new Cell()
            .SetBorder(Border.NO_BORDER)
            .SetPadding(14);

        leftCell.Add(new Paragraph($"Fecha de Emisión: {FormatearFecha(_model.FechaEmision)}").SetMargin(0));
        leftCell.Add(new Paragraph($"RUC Emisor: {ValorOPlaceholder(_model.RucEmisor)}").SetMargin(0));
        leftCell.Add(new Paragraph($"Emisor: {ValorOPlaceholder(_model.RazonSocialEmisor)}").SetMargin(0));
        leftCell.Add(new Paragraph($"Dirección Emisor: {ValorOPlaceholder(_model.DireccionEmisor)}").SetMargin(0));

        Cell rightCell = new Cell()
            .SetBorder(Border.NO_BORDER)
            .SetPadding(14)
            .SetTextAlignment(TextAlignment.RIGHT);

        rightCell.Add(new Paragraph($"Factura Nro: {ValorOPlaceholder(_model.NumeroFactura)}")
            .SetFont(fontBold)
            .SetFontSize(16)
            .SetMargin(0));

        rightCell.Add(new Paragraph($"Timbrado: {ValorOPlaceholder(_model.Timbrado)}").SetMargin(0));
        rightCell.Add(new Paragraph($"Vigencia: {FormatearFecha(_model.VigenciaDesde)} al {FormatearFecha(_model.VigenciaHasta)}").SetMargin(0));

        table.AddCell(leftCell);
        table.AddCell(rightCell);

        return table;
    }

    private Table CrearPanelCliente(PdfFont fontBold)
    {
        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }))
            .UseAllAvailableWidth()
            .SetBorder(new SolidBorder(new DeviceRgb(217, 217, 217), 1))
            .SetMarginBottom(12);

        Cell leftCell = new Cell()
            .SetBorder(Border.NO_BORDER)
            .SetPadding(14);

        leftCell.Add(new Paragraph($"Cliente: {ValorOPlaceholder(_model.NombreCliente)}")
            .SetFont(fontBold)
            .SetMargin(0));

        leftCell.Add(new Paragraph($"Dirección: {ValorOPlaceholder(_model.DireccionCliente)}").SetMargin(0));
        leftCell.Add(new Paragraph($"RUC/Cédula: {ValorOPlaceholder(_model.RucCliente)}")
            .SetFont(fontBold)
            .SetMargin(0));

        leftCell.Add(new Paragraph($"Teléfono: {ValorOPlaceholder(_model.TelefonoCliente)}")
            .SetFont(fontBold)
            .SetMargin(0));

        Cell rightCell = new Cell()
            .SetBorder(Border.NO_BORDER)
            .SetPadding(14);

        rightCell.Add(new Paragraph("Condición de Venta:")
            .SetFont(fontBold)
            .SetMarginBottom(8));

        Table condicionTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }))
            .UseAllAvailableWidth()
            .SetMarginBottom(8);

        condicionTable.AddCell(new Cell()
            .SetBorder(Border.NO_BORDER)
            .Add(new Paragraph(Marcar(_model.CondicionVenta, "CONTADO")).SetMargin(0)));

        condicionTable.AddCell(new Cell()
            .SetBorder(Border.NO_BORDER)
            .Add(new Paragraph(Marcar(_model.CondicionVenta, "CREDITO")).SetMargin(0)));

        rightCell.Add(condicionTable);
        rightCell.Add(new Paragraph($"Tipo de Transacción: {ValorOPlaceholder(_model.TipoTransaccion)}").SetMargin(0));

        table.AddCell(leftCell);
        table.AddCell(rightCell);

        return table;
    }

    private Div CrearPanelDetalles(PdfFont fontBold)
    {
        Div wrapper = new Div()
            .SetBorder(new SolidBorder(new DeviceRgb(217, 217, 217), 1))
            .SetPadding(14)
            .SetMarginBottom(12);

        wrapper.Add(new Paragraph("Detalles")
            .SetFont(fontBold)
            .SetFontSize(12)
            .SetMarginTop(0)
            .SetMarginBottom(10));

        Table table = new Table(UnitValue.CreatePercentArray(new float[]
        {
            11, 28, 9, 9, 13, 11, 9, 5, 6
        }))
        .UseAllAvailableWidth();

        AddHeaderCell(table, "Código", fontBold);
        AddHeaderCell(table, "Descripción", fontBold);
        AddHeaderCell(table, "Unidad", fontBold);
        AddHeaderCell(table, "Cantidad", fontBold);
        AddHeaderCell(table, "Precio\nUnitario", fontBold);
        AddHeaderCell(table, "Descuento", fontBold);
        AddHeaderCell(table, "Exentas", fontBold);
        AddHeaderCell(table, "5%", fontBold);
        AddHeaderCell(table, "10%", fontBold);

        if (_model.Detalles is not null && _model.Detalles.Any())
        {
            foreach (var item in _model.Detalles)
            {
                AddBodyCell(table, item.Codigo, false);
                AddBodyCell(table, item.Descripcion, false);
                AddBodyCell(table, item.Unidad, false);
                AddBodyCell(table, item.Cantidad.ToString("0.##", CultureInfo.InvariantCulture), true);
                AddBodyCell(table, FormatearGs(item.PrecioUnitario), true);
                AddBodyCell(table, FormatearGs(item.Descuento), true);
                AddBodyCell(table, FormatearGs(item.Exentas), true);
                AddBodyCell(table, FormatearGs(item.Iva5), true);
                AddBodyCell(table, FormatearGs(item.Iva10), true);
            }
        }
        else
        {
            Cell emptyCell = new Cell(1, 9)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8)
                .SetBorderBottom(new SolidBorder(new DeviceRgb(229, 231, 235), 1))
                .SetBorderLeft(Border.NO_BORDER)
                .SetBorderRight(Border.NO_BORDER)
                .SetBorderTop(Border.NO_BORDER)
                .Add(new Paragraph("Sin detalles").SetMargin(0));

            table.AddCell(emptyCell);
        }

        wrapper.Add(table);

        Table totalsTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }))
     .SetWidth(220)
     .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
     .SetMarginTop(8);

        totalsTable.AddCell(new Cell()
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.RIGHT)
            .Add(new Paragraph("Total IVA:").SetFont(fontBold)));

        totalsTable.AddCell(new Cell()
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.RIGHT)
            .Add(new Paragraph(FormatearGs(_model.TotalIva))));

        totalsTable.AddCell(new Cell()
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.RIGHT)
            .Add(new Paragraph("Total General:").SetFont(fontBold)));

        totalsTable.AddCell(new Cell()
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.RIGHT)
            .Add(new Paragraph(FormatearGs(_model.TotalGeneral))));


        wrapper.Add(totalsTable);

        return wrapper;
    }

    private Div CrearPanelAnulacion(PdfFont fontBold)
    {
        Div wrapper = new Div()
            .SetBorder(new SolidBorder(new DeviceRgb(217, 217, 217), 1))
            .SetPadding(14);

        wrapper.Add(new Paragraph("Datos de anulación")
            .SetFont(fontBold)
            .SetFontSize(11)
            .SetFontColor(new DeviceRgb(211, 47, 47))
            .SetMarginTop(0)
            .SetMarginBottom(4));

        wrapper.Add(new Paragraph($"Fecha de anulación: {FormatearFecha(_model.FechaAnulacion)}").SetMargin(0));
        wrapper.Add(new Paragraph($"Usuario: {ValorOPlaceholder(_model.UsuarioAnulacion)}").SetMargin(0));
        wrapper.Add(new Paragraph($"Motivo: {ValorOPlaceholder(_model.MotivoAnulacion)}").SetMargin(0));

        return wrapper;
    }

    private static void AddHeaderCell(Table table, string text, PdfFont fontBold)
    {
        table.AddHeaderCell(new Cell()
            .SetBorderTop(Border.NO_BORDER)
            .SetBorderLeft(Border.NO_BORDER)
            .SetBorderRight(Border.NO_BORDER)
            .SetBorderBottom(new SolidBorder(new DeviceRgb(217, 217, 217), 1))
            .SetPaddingTop(8)
            .SetPaddingBottom(8)
            .SetPaddingLeft(6)
            .SetPaddingRight(6)
            .Add(new Paragraph(text)
                .SetFont(fontBold)
                .SetFontSize(9)
                .SetMargin(0)));
    }

    private static void AddBodyCell(Table table, string? text, bool alignRight)
    {
        Cell cell = new Cell()
            .SetBorderTop(Border.NO_BORDER)
            .SetBorderLeft(Border.NO_BORDER)
            .SetBorderRight(Border.NO_BORDER)
            .SetBorderBottom(new SolidBorder(new DeviceRgb(229, 231, 235), 1))
            .SetPaddingTop(8)
            .SetPaddingBottom(8)
            .SetPaddingLeft(6)
            .SetPaddingRight(6);

        if (alignRight)
            cell.SetTextAlignment(TextAlignment.RIGHT);

        cell.Add(new Paragraph(text ?? string.Empty)
            .SetFontSize(9)
            .SetMargin(0));

        table.AddCell(cell);
    }

    private static void AgregarMarcaAgua(PdfDocument pdf, PdfFont fontBold)
    {
        for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
        {
            var page = pdf.GetPage(i);
            var pageSize = page.GetPageSize();

            PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);
            iText.Layout.Canvas canvas = new(pdfCanvas, pageSize);

            canvas.ShowTextAligned(
                new Paragraph("ANULADA")
                    .SetFont(fontBold)
                    .SetFontSize(74)
                    .SetFontColor(new DeviceRgb(211, 47, 47)),
                pageSize.GetWidth() / 2,
                pageSize.GetHeight() / 2,
                i,
                TextAlignment.CENTER,
                VerticalAlignment.MIDDLE,
                (float)(Math.PI / 4));

            canvas.Close();
        }
    }

    private static string FormatearGs(decimal monto) =>
        string.Format(new CultureInfo("es-PY"), "{0:N0}", monto);

    private static string FormatearFecha(DateTime? fecha) =>
        fecha?.ToString("d/M/yyyy") ?? string.Empty;

    private static string ValorOPlaceholder(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? "-" : valor;

    private static string Marcar(string? condicionVenta, string valor)
    {
        bool marcado = string.Equals(condicionVenta, valor, StringComparison.OrdinalIgnoreCase);
        return marcado ? $"(X) {Capitalizar(valor)}" : $"( ) {Capitalizar(valor)}";
    }

    private static string Capitalizar(string valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? string.Empty
            : valor[..1].ToUpper() + valor[1..].ToLower();

    private bool EsAnulada() =>
        string.Equals(_model.EstadoFactura, "Anulada", StringComparison.OrdinalIgnoreCase);
}
