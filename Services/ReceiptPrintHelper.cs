using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using GymFinanceBillingWpf.Models;

namespace GymFinanceBillingWpf.Services;

public static class ReceiptPrintHelper
{
    public static void PrintReceipt(Invoice invoice)
    {
        FlowDocument doc = CreateReceiptDocument(invoice);

        PrintDialog printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            // Set document dimensions to fit the printer printable page width
            doc.PageWidth = printDialog.PrintableAreaWidth;
            doc.PageHeight = double.NaN; // Automatic height adjustment
            doc.PagePadding = new Thickness(30, 25, 30, 25);
            doc.ColumnWidth = printDialog.PrintableAreaWidth - 60;

            IDocumentPaginatorSource idpSource = doc;
            printDialog.PrintDocument(idpSource.DocumentPaginator, $"Receipt_{invoice.InvoiceNumber}");
        }
    }

    public static FlowDocument CreateReceiptDocument(Invoice invoice)
    {
        FlowDocument doc = new FlowDocument();
        doc.FontFamily = new FontFamily("Segoe UI");
        doc.FontSize = 12;
        doc.PagePadding = new Thickness(45);
        doc.Foreground = Brushes.Black;
        doc.Background = Brushes.White;

        // Title Header
        Paragraph title = new Paragraph(new Run("AeroGym"))
        {
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 2)
        };
        doc.Blocks.Add(title);

        Paragraph subtitle = new Paragraph(new Run("Premium Fitness & Health Club\n123 Fitness Ave, Gym City\nPhone: +91 98765 43210 | Email: billing@aerogym.com"))
        {
            FontSize = 10,
            Foreground = Brushes.DimGray,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };
        doc.Blocks.Add(subtitle);

        doc.Blocks.Add(CreateDivider());

        // Slip Details Summary Table
        Table tableInfo = new Table { CellSpacing = 0, Margin = new Thickness(0, 10, 0, 10) };
        tableInfo.Columns.Add(new TableColumn { Width = new GridLength(1.1, GridUnitType.Star) });
        tableInfo.Columns.Add(new TableColumn { Width = new GridLength(0.9, GridUnitType.Star) });

        TableRowGroup groupInfo = new TableRowGroup();
        groupInfo.Rows.Add(CreateKeyValueRow("Slip No:", invoice.InvoiceNumber, "Customer Name:", invoice.Member?.FullName ?? "N/A"));
        groupInfo.Rows.Add(CreateKeyValueRow("Reg No:", invoice.Member?.RegNo ?? "N/A", "Mobile No:", invoice.Member?.Phone ?? "N/A"));
        groupInfo.Rows.Add(CreateKeyValueRow("Slip Date:", invoice.IssueDate.ToString("yyyy-MM-dd"), "Due From:", invoice.ServicePeriodStart?.ToString("yyyy-MM-dd") ?? "N/A"));
        groupInfo.Rows.Add(CreateKeyValueRow("Status:", invoice.Status.ToString().ToUpper(), "Due To:", invoice.ServicePeriodEnd?.ToString("yyyy-MM-dd") ?? "N/A"));
        tableInfo.RowGroups.Add(groupInfo);
        doc.Blocks.Add(tableInfo);

        doc.Blocks.Add(CreateDivider());

        // Summary items header label
        Paragraph itemsHeader = new Paragraph(new Run("ITEMS SUMMARY"))
        {
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 12, 0, 6)
        };
        doc.Blocks.Add(itemsHeader);

        // Items Breakdown Table
        Table tableItems = new Table { CellSpacing = 0, Margin = new Thickness(0, 5, 0, 10) };
        tableItems.Columns.Add(new TableColumn { Width = new GridLength(3.2, GridUnitType.Star) }); // Description
        tableItems.Columns.Add(new TableColumn { Width = new GridLength(0.8, GridUnitType.Star) }); // Quantity
        tableItems.Columns.Add(new TableColumn { Width = new GridLength(1.0, GridUnitType.Star) }); // Rate
        tableItems.Columns.Add(new TableColumn { Width = new GridLength(1.2, GridUnitType.Star) }); // Total

        TableRowGroup groupItems = new TableRowGroup();
        
        // Columns header row
        TableRow headerRow = new TableRow();
        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Description")) { FontWeight = FontWeights.Bold }));
        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Qty")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }));
        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Rate")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right }));
        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Total")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right }));
        groupItems.Rows.Add(headerRow);

        // Data rows iteration
        foreach (var item in invoice.Items)
        {
            TableRow row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Description))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString())) { TextAlignment = TextAlignment.Center }));
            row.Cells.Add(new TableCell(new Paragraph(new Run($"₹{item.UnitPrice:N2}")) { TextAlignment = TextAlignment.Right }));
            row.Cells.Add(new TableCell(new Paragraph(new Run($"₹{item.TotalPrice:N2}")) { TextAlignment = TextAlignment.Right }));
            groupItems.Rows.Add(row);
        }
        tableItems.RowGroups.Add(groupItems);
        doc.Blocks.Add(tableItems);

        doc.Blocks.Add(CreateDivider());

        // Payment Totals Breakdown Table
        Table tableTotals = new Table { CellSpacing = 0, Margin = new Thickness(0, 10, 0, 10) };
        tableTotals.Columns.Add(new TableColumn { Width = new GridLength(3.5, GridUnitType.Star) });
        tableTotals.Columns.Add(new TableColumn { Width = new GridLength(1.7, GridUnitType.Star) });

        TableRowGroup groupTotals = new TableRowGroup();
        groupTotals.Rows.Add(CreateTotalRow("Fee Subtotal:", $"₹{invoice.Total:N2}"));
        
        if (invoice.AdmissionFee > 0)
        {
            groupTotals.Rows.Add(CreateTotalRow("Admission Fee:", $"₹{invoice.AdmissionFee:N2}"));
        }
        if (invoice.AdmissionFeeDiscount > 0)
        {
            groupTotals.Rows.Add(CreateTotalRow("Admission Discount:", $"-₹{invoice.AdmissionFeeDiscount:N2}"));
        }
        
        groupTotals.Rows.Add(CreateTotalRow("Grand Total:", $"₹{invoice.GrandTotal:N2}", isBold: true));
        
        if (invoice.CashAmount > 0)
            groupTotals.Rows.Add(CreateTotalRow("Cash Paid:", $"₹{invoice.CashAmount:N2}"));
        if (invoice.UpiAmount > 0)
            groupTotals.Rows.Add(CreateTotalRow("UPI Paid:", $"₹{invoice.UpiAmount:N2}"));
        if (invoice.CardAmount > 0)
            groupTotals.Rows.Add(CreateTotalRow("Card Paid:", $"₹{invoice.CardAmount:N2}"));

        groupTotals.Rows.Add(CreateTotalRow("Balance Due:", $"₹{invoice.PendingAmount:N2}", isBold: true, isRed: invoice.PendingAmount > 0));
        
        tableTotals.RowGroups.Add(groupTotals);
        doc.Blocks.Add(tableTotals);

        if (!string.IsNullOrEmpty(invoice.Narration))
        {
            Paragraph narration = new Paragraph(new Run($"Remarks/Narration:\n{invoice.Narration}"))
            {
                FontSize = 9.5,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 12, 0, 12)
            };
            doc.Blocks.Add(narration);
        }

        doc.Blocks.Add(CreateDivider());

        // Authorized Signature panel
        Paragraph signature = new Paragraph(new Run("\n\n\n_______________________\nAuthorized Signature"))
        {
            TextAlignment = TextAlignment.Right,
            FontSize = 10,
            Margin = new Thickness(0, 10, 0, 20)
        };
        doc.Blocks.Add(signature);

        Paragraph thankYou = new Paragraph(new Run("Thank you for your membership at AeroGym!\nKeep training hard!"))
        {
            TextAlignment = TextAlignment.Center,
            FontSize = 10,
            Foreground = Brushes.DimGray,
            Margin = new Thickness(0, 10, 0, 0)
        };
        doc.Blocks.Add(thankYou);

        return doc;
    }

    private static Block CreateDivider()
    {
        Paragraph p = new Paragraph();
        p.Margin = new Thickness(0, 4, 0, 4);
        p.Inlines.Add(new Run("----------------------------------------------------------------------------------------------------") 
        { 
            Foreground = Brushes.LightGray, 
            FontWeight = FontWeights.UltraLight 
        });
        return p;
    }

    private static TableRow CreateKeyValueRow(string key1, string val1, string key2, string val2)
    {
        TableRow row = new TableRow();
        
        // Item 1 Pair
        Paragraph p1 = new Paragraph();
        p1.Margin = new Thickness(0, 2, 0, 2);
        p1.Inlines.Add(new Run(key1) { FontWeight = FontWeights.SemiBold, Foreground = Brushes.Gray });
        p1.Inlines.Add(new Run(" " + val1));
        row.Cells.Add(new TableCell(p1));

        // Item 2 Pair
        Paragraph p2 = new Paragraph();
        p2.Margin = new Thickness(0, 2, 0, 2);
        p2.Inlines.Add(new Run(key2) { FontWeight = FontWeights.SemiBold, Foreground = Brushes.Gray });
        p2.Inlines.Add(new Run(" " + val2));
        row.Cells.Add(new TableCell(p2));

        return row;
    }

    private static TableRow CreateTotalRow(string label, string val, bool isBold = false, bool isRed = false)
    {
        TableRow row = new TableRow();
        
        Paragraph pLabel = new Paragraph(new Run(label)) 
        { 
            TextAlignment = TextAlignment.Right,
            FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal
        };
        row.Cells.Add(new TableCell(pLabel));

        Paragraph pVal = new Paragraph(new Run(val)) 
        { 
            TextAlignment = TextAlignment.Right,
            FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
            Foreground = isRed ? Brushes.Red : Brushes.Black
        };
        row.Cells.Add(new TableCell(pVal));

        return row;
    }
}
