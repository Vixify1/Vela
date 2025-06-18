using HRWebApp.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace HRWebApp.Helper
{
    public class SalaryLetterHelper
    {
        public byte[] GenerateSalaryLetterPdf(SalaryLetterViewModel model)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Create PDF document
                var document = new Document(PageSize.A4, 40, 40, 60, 60);
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Define custom colors
                var veryLightGray = new BaseColor(248, 248, 248); // Very light gray
                var lightGray = BaseColor.LightGray;
                var mediumGray = new BaseColor(220, 220, 220); // Medium gray

                // Add company header
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DarkGray);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.Black);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.Black);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.Black);

                // Company name
                var companyTitle = new Paragraph(model.CompanyName, titleFont);
                companyTitle.Alignment = Element.ALIGN_CENTER;
                document.Add(companyTitle);
                document.Add(new Paragraph(" "));

                // Company address
                var companyAddress = new Paragraph(model.CompanyAddress, normalFont);
                companyAddress.Alignment = Element.ALIGN_CENTER;
                document.Add(companyAddress);
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));

                // Letter title
                var letterTitle = new Paragraph("SALARY CERTIFICATE", headerFont);
                letterTitle.Alignment = Element.ALIGN_CENTER;
                document.Add(letterTitle);
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));

                // Date
                var dateText = new Paragraph($"Date: {model.GeneratedDate:MMMM dd, yyyy}", normalFont);
                document.Add(dateText);
                document.Add(new Paragraph(" "));

                // Employee information
                document.Add(new Paragraph("TO WHOM IT MAY CONCERN:", boldFont));
                document.Add(new Paragraph(" "));

                var introText = new Paragraph($"This is to certify that {model.EmployeeName} is employed with {model.CompanyName} " +
                                            $"in the {model.DepartmentName} department.", normalFont);
                document.Add(introText);
                document.Add(new Paragraph(" "));

                // Salary details table
                var table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1, 1 });

                // Add table headers
                var headerCell1 = new PdfPCell(new Phrase("PARTICULARS", boldFont));
                headerCell1.BackgroundColor = mediumGray;
                headerCell1.Padding = 8;
                table.AddCell(headerCell1);

                var headerCell2 = new PdfPCell(new Phrase("AMOUNT", boldFont));
                headerCell2.BackgroundColor = mediumGray;
                headerCell2.Padding = 8;
                table.AddCell(headerCell2);

                // Add salary details
                AddTableRow(table, "Period:", model.MonthYearDisplay, normalFont);
                AddTableRow(table, "Hourly Rate:", $"{model.HourlyRate:N0} LEK", normalFont);
                AddTableRow(table, "Standard Hours:", $"{model.StandardHours:F1} hrs", normalFont);
                AddTableRow(table, "Holiday Hours (1.5x):", $"{model.HolidayHours:F1} hrs", normalFont);
                AddTableRow(table, "Sunday Hours (1.75x):", $"{model.SundayHours:F1} hrs", normalFont);
                AddTableRow(table, "Total Hours:", $"{model.TotalHours:F1} hrs", normalFont);
                AddTableRow(table, "", "", normalFont); // Empty row for spacing
                
                // Pay breakdown
                AddTableRow(table, "Standard Pay:", $"{model.StandardPay:N0} LEK", normalFont);
                AddTableRow(table, "Holiday Pay:", $"{model.HolidayPay:N0} LEK", normalFont);
                AddTableRow(table, "Sunday Pay:", $"{model.SundayPay:N0} LEK", normalFont);

                // Gross salary row with bold
                var grossCell1 = new PdfPCell(new Phrase("Gross Salary:", boldFont));
                grossCell1.Padding = 8;
                grossCell1.BackgroundColor = lightGray;
                table.AddCell(grossCell1);
                var grossCell2 = new PdfPCell(new Phrase($"{model.GrossSalary:N0} LEK", boldFont));
                grossCell2.Padding = 8;
                grossCell2.BackgroundColor = lightGray;
                table.AddCell(grossCell2);

                // Add empty row for spacing
                AddTableRow(table, "", "", normalFont);

                // Deductions section header
                var deductionHeaderCell1 = new PdfPCell(new Phrase("DEDUCTIONS:", boldFont));
                deductionHeaderCell1.Padding = 8;
                deductionHeaderCell1.BackgroundColor = veryLightGray;
                table.AddCell(deductionHeaderCell1);
                var deductionHeaderCell2 = new PdfPCell(new Phrase("", boldFont));
                deductionHeaderCell2.Padding = 8;
                deductionHeaderCell2.BackgroundColor = veryLightGray;
                table.AddCell(deductionHeaderCell2);

                // Individual deductions
                AddTableRow(table, "Social Security (9.5%):", $"{model.SocialSecurityDeduction:N0} LEK", normalFont);
                AddTableRow(table, "Health Insurance (1.7%):", $"{model.HealthInsuranceDeduction:N0} LEK", normalFont);
                AddTableRow(table, "Income Tax:", $"{model.IncomeTaxDeduction:N0} LEK", normalFont);

                // Total deductions row with bold
                var totalDeductionsCell1 = new PdfPCell(new Phrase("Total Deductions:", boldFont));
                totalDeductionsCell1.Padding = 8;
                totalDeductionsCell1.BackgroundColor = veryLightGray;
                table.AddCell(totalDeductionsCell1);
                var totalDeductionsCell2 = new PdfPCell(new Phrase($"{model.TotalDeductions:N0} LEK", boldFont));
                totalDeductionsCell2.Padding = 8;
                totalDeductionsCell2.BackgroundColor = veryLightGray;
                table.AddCell(totalDeductionsCell2);

                // Add empty row for spacing
                AddTableRow(table, "", "", normalFont);

                // Net salary row with bold and highlighted
                var netCell1 = new PdfPCell(new Phrase("Net Salary:", boldFont));
                netCell1.Padding = 8;
                netCell1.BackgroundColor = lightGray;
                table.AddCell(netCell1);
                var netCell2 = new PdfPCell(new Phrase($"{model.NetSalary:N0} LEK", boldFont));
                netCell2.Padding = 8;
                netCell2.BackgroundColor = lightGray;
                table.AddCell(netCell2);

                document.Add(table);
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));

                // Add calculation explanation
                var calculationNote = new Paragraph("Note: Net salary is calculated as Gross Salary minus all applicable deductions as per Albanian tax regulations.", 
                    FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DarkGray));
                calculationNote.Alignment = Element.ALIGN_JUSTIFIED;
                document.Add(calculationNote);
                document.Add(new Paragraph(" "));

                // Closing statement
                var closingText = new Paragraph("This certificate is issued for official purposes upon the request of the employee.", normalFont);
                document.Add(closingText);
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));

                // Signature section
                document.Add(new Paragraph("Sincerely,", normalFont));
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("_________________________", normalFont));
                document.Add(new Paragraph("HR Department", normalFont));
                document.Add(new Paragraph(model.CompanyName, normalFont));

                document.Close();
                return memoryStream.ToArray();
            }
        }

        private void AddTableRow(PdfPTable table, string label, string value, Font font)
        {
            var cell1 = new PdfPCell(new Phrase(label, font));
            cell1.Padding = 8;
            table.AddCell(cell1);

            var cell2 = new PdfPCell(new Phrase(value, font));
            cell2.Padding = 8;
            table.AddCell(cell2);
        }
    }
}