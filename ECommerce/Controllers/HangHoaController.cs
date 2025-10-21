using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers
{
	public class HangHoaController : Controller
	{
		private readonly EcommerceContext db;

		public HangHoaController(EcommerceContext conetxt)
		{
			db = conetxt;
		}

		public IActionResult Index(int? loai, int? page)
		{
			var hangHoas = db.HangHoas.AsQueryable();

			if (loai.HasValue)
			{
				hangHoas = hangHoas.Where(p => p.MaLoai == loai.Value);
				ViewData["CurrentLoai"] = loai.Value;
			}

			const int pageSize = 6;

			var totalProducts = hangHoas.Count();
			var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

			if (page == null) { page = 1; }
			int skipN = (page.Value - 1) * pageSize;

			var result = hangHoas.OrderByDescending(p => p.TenHh)
								 .Skip(skipN)
								 .Take(pageSize)
								 .Select(p => new HangHoaVM
								 {
									 MaHh = p.MaHh,
									 TenHH = p.TenHh,
									 DonGia = p.DonGia ?? 0,
									 Hinh = p.Hinh ?? "",
									 MoTaNgan = p.MoTaDonVi ?? "",
									 TenLoai = p.MaLoaiNavigation.TenLoai
								 }).ToList();

			ViewBag.TotalPages = totalPages;
			ViewBag.CurrentPage = page;

			return View(result);
		}

		public IActionResult Search(string? query)
		{
			var hangHoas = db.HangHoas.AsQueryable();

			if (query != null)
			{
				hangHoas = hangHoas.Where(p => p.TenHh.Contains(query));
			}

			var result = hangHoas.Select(p => new HangHoaVM
			{
				MaHh = p.MaHh,
				TenHH = p.TenHh,
				DonGia = p.DonGia ?? 0,
				Hinh = p.Hinh ?? "",
				MoTaNgan = p.MoTaDonVi ?? "",
				TenLoai = p.MaLoaiNavigation.TenLoai
			});
			return View(result);
		}

		const int pageSize = 6;
		public IActionResult PhanTrang(int? page)
		{
			if (page == null) { page = 1; }
			int skipN = (page.Value - 1) * pageSize;
			var dsHangHoa = db.HangHoas.OrderByDescending(p => p.TenHh).Skip(skipN).Take(pageSize).ToList();

			var totalProducts = db.HangHoas.Count();
			var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

			ViewBag.TotalPages = totalPages;

			return View("Index", dsHangHoa.Select(hh => new HangHoaVM
			{
				MaHh = hh.MaHh,
				TenHH = hh.TenHh,
				Hinh = hh.Hinh ?? "",
				DonGia = hh.DonGia ?? 0
			}));

		}
		
		public IActionResult Detail(int id)
		{
			var data = db.HangHoas
				.Include(p => p.MaLoaiNavigation)
				.SingleOrDefault(p => p.MaHh == id);
			if (data == null)
			{
				TempData["Message"] = $"Không thấy sản phẩm có mã {id}";
				return Redirect("/404");
			}

			var result = new ChiTietHangHoaVM
			{
				MaHh = data.MaHh,
				TenHH = data.TenHh,
				DonGia = data.DonGia ?? 0,
				ChiTiet = data.MoTa ?? string.Empty,
				Hinh = data.Hinh ?? string.Empty,
				MoTaNgan = data.MoTaDonVi ?? string.Empty,
				TenLoai = data.MaLoaiNavigation.TenLoai,
				SoLuongTon = 10,//tính sau
				DiemDanhGia = 5,//check sau
			};
			return View(result);
		}
	}
}
