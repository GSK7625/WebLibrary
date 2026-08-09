using System.ComponentModel.DataAnnotations;
using Library.Business.DTOs;
using Library.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Library.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookApplicationService _bookService;

    public BooksController(IBookApplicationService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// Lay danh sach tat ca sach
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
    }

    /// <summary>
    /// Tinh thu phi tra han (Chuan OCP - Strategy Pattern)
    /// </summary>
    [HttpGet("{id}/fee-preview")]
    public async Task<ActionResult<FeePreviewDto>> PreviewFee(int id, [FromQuery, Range(0, 365, ErrorMessage = "So ngay tre phai tu 0 den 365 ngay")] int daysLate)
    {
        var result = await _bookService.PreviewFeeAsync(id, daysLate);
        return Ok(result);
    }

    /// <summary>
    /// Tinh thu phi tra han (Legacy - Vi pham OCP)
    /// </summary>
    [HttpGet("{id}/legacy-fee-preview")]
    public async Task<ActionResult<FeePreviewDto>> PreviewFeeLegacy(int id, [FromQuery, Range(0, 365, ErrorMessage = "So ngay tre phai tu 0 den 365 ngay")] int daysLate)
    {
        var result = await _bookService.PreviewLegacyFeeAsync(id, daysLate);
        return Ok(result);
    }
}
