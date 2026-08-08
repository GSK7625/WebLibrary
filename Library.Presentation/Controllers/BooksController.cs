using System.ComponentModel.DataAnnotations;
using Library.Business.DTOs;
using Library.Business.Services.Interfaces;
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
    /// Lấy danh sách tất cả sách
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
    }

    /// <summary>
    /// Tính thử phí trả hạn (Chuẩn OCP - Strategy Pattern)
    /// </summary>
    [HttpGet("{id}/fee-preview")]
    public async Task<ActionResult<FeePreviewDto>> PreviewFee(int id, [FromQuery, Range(0, 365, ErrorMessage = "Số ngày trễ phải từ 0 đến 365 ngày")] int daysLate)
    {
        var result = await _bookService.PreviewFeeAsync(id, daysLate);
        return Ok(result);
    }

    /// <summary>
    /// Tính thử phí trả hạn (Legacy - Vi phạm OCP)
    /// </summary>
    [HttpGet("{id}/legacy-fee-preview")]
    public async Task<ActionResult<FeePreviewDto>> PreviewFeeLegacy(int id, [FromQuery, Range(0, 365, ErrorMessage = "Số ngày trễ phải từ 0 đến 365 ngày")] int daysLate)
    {
        var result = await _bookService.PreviewLegacyFeeAsync(id, daysLate);
        return Ok(result);
    }
}
