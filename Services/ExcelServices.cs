using HungryHub.Data;
using HungryHub.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace HungryHub.Services
{
    public class ExcelService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ExcelService(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
           
            ExcelPackage.License.SetNonCommercialPersonal("HungryHub");
        }

        public string GenerateExcelReport()
        {
            string folder = Path.Combine(_env.WebRootPath, "exports");
            Directory.CreateDirectory(folder);
            string filePath = Path.Combine(folder, "HungryHub_Data.xlsx");

            using var package = new ExcelPackage();

            CreateUsersSheet(package);
            CreateOrdersSheet(package);
            CreateOrderItemsSheet(package);

            package.SaveAs(new FileInfo(filePath));
            return filePath;
        }

        private void CreateUsersSheet(ExcelPackage package)
        {
            var ws = package.Workbook.Worksheets.Add("Users");

            string[] headers = {
                "User ID", "First Name", "Last Name",
                "Email", "Phone", "Gender",
                "Date of Birth", "Registered At"
            };

            StyleHeader(ws, headers, "#E84545");

            var users = _db.Users.ToList();
            int row = 2;
            foreach (var u in users)
            {
                ws.Cells[row, 1].Value = u.UserId;
                ws.Cells[row, 2].Value = u.FirstName;
                ws.Cells[row, 3].Value = u.LastName;
                ws.Cells[row, 4].Value = u.Email;
                ws.Cells[row, 5].Value = u.PhoneNumber;
                ws.Cells[row, 6].Value = u.Gender;
                ws.Cells[row, 7].Value = u.DateOfBirth.ToString("yyyy-MM-dd");
                ws.Cells[row, 8].Value = u.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                StyleDataRow(ws, row, headers.Length);
                row++;
            }

            AutoFitColumns(ws, headers.Length);
        }

        private void CreateOrdersSheet(ExcelPackage package)
        {
            var ws = package.Workbook.Worksheets.Add("Orders");

            string[] headers = {
                "Order ID", "User ID", "Restaurant",
                "Total (BDT)", "Delivery Fee (BDT)", "Grand Total (BDT)",
                "Status", "Delivery Address", "Note", "Order Date"
            };

            StyleHeader(ws, headers, "#1A1A2E");

            var orders = _db.Orders.ToList();
            int row = 2;
            foreach (var o in orders)
            {
                ws.Cells[row, 1].Value = o.OrderId;
                ws.Cells[row, 2].Value = o.UserId;
                ws.Cells[row, 3].Value = o.RestaurantName;
                ws.Cells[row, 4].Value = o.TotalAmount;
                ws.Cells[row, 5].Value = o.DeliveryFee;
                ws.Cells[row, 6].Value = o.GrandTotal;
                ws.Cells[row, 7].Value = o.Status;
                ws.Cells[row, 8].Value = o.DeliveryAddress;
                ws.Cells[row, 9].Value = o.OrderNote;
                ws.Cells[row, 10].Value = o.OrderDate.ToString("yyyy-MM-dd HH:mm");

                var statusCell = ws.Cells[row, 7];
                statusCell.Style.Font.Color.SetColor(
                    o.Status == "Confirmed" ? Color.Green :
                    o.Status == "Cancelled" ? Color.Red :
                    o.Status == "Delivered" ? Color.Blue : Color.Orange);
                statusCell.Style.Font.Bold = true;

                StyleDataRow(ws, row, headers.Length);
                row++;
            }

            AutoFitColumns(ws, headers.Length);
        }

        private void CreateOrderItemsSheet(ExcelPackage package)
        {
            var ws = package.Workbook.Worksheets.Add("Order Items");

            string[] headers = {
                "Item ID", "Order ID", "Item Name",
                "Quantity", "Unit Price (BDT)", "Subtotal (BDT)"
            };

            StyleHeader(ws, headers, "#333366");

            var items = _db.OrderItems.ToList();
            int row = 2;
            foreach (var i in items)
            {
                ws.Cells[row, 1].Value = i.OrderItemId;
                ws.Cells[row, 2].Value = i.OrderId;
                ws.Cells[row, 3].Value = i.ItemName;
                ws.Cells[row, 4].Value = i.Quantity;
                ws.Cells[row, 5].Value = i.UnitPrice;
                ws.Cells[row, 6].Value = i.SubTotal;
                StyleDataRow(ws, row, headers.Length);
                row++;
            }

            AutoFitColumns(ws, headers.Length);
        }

        private void StyleHeader(ExcelWorksheet ws,
                                  string[] headers, string hexColor)
        {
            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = ws.Cells[1, col];
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 11;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(
                    ColorTranslator.FromHtml(hexColor));
                cell.Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                cell.Style.Border.BorderAround(
                    ExcelBorderStyle.Thin, Color.White);
            }
            ws.Row(1).Height = 28;
        }

        private void StyleDataRow(ExcelWorksheet ws, int row, int colCount)
        {
            for (int col = 1; col <= colCount; col++)
            {
                var cell = ws.Cells[row, col];
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(
                    row % 2 == 0
                        ? ColorTranslator.FromHtml("#F9F9F9")
                        : Color.White);
                cell.Style.Border.BorderAround(
                    ExcelBorderStyle.Hair, Color.LightGray);
            }
            ws.Row(row).Height = 20;
        }

        private void AutoFitColumns(ExcelWorksheet ws, int colCount)
        {
            for (int col = 1; col <= colCount; col++)
                ws.Column(col).AutoFit();
        }
    }
}