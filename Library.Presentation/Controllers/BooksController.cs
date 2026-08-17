using System.ComponentModel.DataAnnotations;
using Library.Business.DTOs;
using Library.Business.Enums;
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
    /// Lấy danh sách tất cả các đầu sách
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
    }

    /// <summary>
    /// Tính thử phí trả hạn nâng cao (Chuẩn OCP - Dynamic Predicate Strategy Pattern & Rule Auditing)
    /// </summary>
    /// <param name="id">Id của sách</param>
    /// <param name="daysLate">Số ngày trễ (0 - 365)</param>
    /// <param name="memberType">Loại độc giả: 1 (Standard), 2 (Student), 3 (VIP), 4 (Staff)</param>
    [HttpGet("{id}/fee-preview")]
    public async Task<ActionResult<FeePreviewDto>> PreviewFee(
        int id,
        [FromQuery, Range(0, 365, ErrorMessage = "Số ngày trễ phải từ 0 đến 365 ngày")] int daysLate,
        [FromQuery] MemberType memberType = MemberType.Standard)
    {
        var result = await _bookService.PreviewFeeAsync(id, daysLate, memberType);
        return Ok(result);
    }

    /// <summary>
    /// Tính thử phí trả hạn nâng cao (Legacy - Vi phạm OCP: Monolithic Switch-Case & If-Else)
    /// </summary>
    /// <param name="id">Id của sách</param>
    /// <param name="daysLate">Số ngày trễ (0 - 365)</param>
    /// <param name="memberType">Loại độc giả: 1 (Standard), 2 (Student), 3 (VIP), 4 (Staff)</param>
    [HttpGet("{id}/legacy-fee-preview")]
    public async Task<ActionResult<FeePreviewDto>> PreviewFeeLegacy(
        int id,
        [FromQuery, Range(0, 365, ErrorMessage = "Số ngày trễ phải từ 0 đến 365 ngày")] int daysLate,
        [FromQuery] MemberType memberType = MemberType.Standard)
    {
        var result = await _bookService.PreviewLegacyFeeAsync(id, daysLate, memberType);
        return Ok(result);
    }
}
