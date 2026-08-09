using Library.Business.DTOs;
using Library.Business.Legacy;
using Library.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Library.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BorrowController : ControllerBase
{
    private readonly IBorrowApplicationService _borrowService;
    private readonly BadBorrowManager _badBorrowManager;

    public BorrowController(
        IBorrowApplicationService borrowService,
        BadBorrowManager badBorrowManager)
    {
        _borrowService = borrowService;
        _badBorrowManager = badBorrowManager;
    }

    /// <summary>
    /// Lay toan bo lich su muon / tra sach
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BorrowRecordDto>>> GetAll()
    {
        var records = await _borrowService.GetAllBorrowRecordsAsync();
        return Ok(records);
    }

    /// <summary>
    /// Muon sach (Tao BorrowRecord)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BorrowRecordDto>> BorrowBook([FromBody] BorrowRequestDto request)
    {
        var result = await _borrowService.BorrowBookAsync(request);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    /// <summary>
    /// Tra sach - Chuan SRP (Phan tach ro rang giua Repository, Fee Strategy & Application Service)
    /// </summary>
    [HttpPost("{id}/return")]
    public async Task<ActionResult<ReturnBookResponseDto>> ReturnBook(int id, [FromBody] ReturnBookRequestDto? request)
    {
        var result = await _borrowService.ReturnBookByBorrowRecordIdAsync(id, request?.ReturnedDate);
        return Ok(result);
    }

    /// <summary>
    /// Tra sach - Demo VI PHAM SRP (1 Class BadBorrowManager om 5 trach nhiem)
    /// </summary>
    [HttpPost("{id}/return-srp-violation")]
    public async Task<IActionResult> ReturnBookSrpViolation(int id, [FromBody] ReturnBookRequestDto? request)
    {
        var result = await _badBorrowManager.ReturnBookBadSRPAsync(id, request?.ReturnedDate);
        return Ok(result);
    }
}
