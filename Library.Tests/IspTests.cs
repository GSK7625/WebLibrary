using Library.Business.Isp;
using Library.Business.Legacy;
using Xunit;

namespace Library.Tests;

public class IspTests
{
    [Fact]
    public void IspViolation_WhenClientForcedToImplementFatInterface_ThrowsNotImplementedException()
    {
        // ARRANGE: Client Kiosk công cộng bị ép implement IFatLibraryOperations
        IFatLibraryOperations badKiosk = new BadGuestKioskClient();

        // ACT & ASSERT: Hàm tìm kiếm hoạt động bình thường
        var searchResults = badKiosk.SearchBooks("Clean");
        Assert.NotEmpty(searchResults);

        // ACT & ASSERT: Các hàm thừa thãi bị ép implement sẽ ném NotImplementedException
        Assert.Throws<NotImplementedException>(() => badKiosk.BorrowBook(1, "Nguyen Van A"));
        Assert.Throws<NotImplementedException>(() => badKiosk.DeleteBookFromSystem(1));
        Assert.Throws<NotImplementedException>(() => badKiosk.AuditTotalInventory());
        Assert.Throws<NotImplementedException>(() => badKiosk.PrintBarcodeSticker(1));
    }

    [Fact]
    public void IspCompliant_GuestKiosk_OnlyDependsOnSearchInterface()
    {
        // ARRANGE: CleanGuestKioskService chỉ hiện thực IBookSearchService
        IBookSearchService searchService = new CleanGuestKioskService();

        // ACT
        var books = searchService.SearchBooks("Patterns");
        var book = searchService.GetBookDetails(1);

        // ASSERT: Hoạt động trơn tru, không có phương thức thừa
        Assert.Single(books);
        Assert.NotNull(book);
        Assert.Equal("Clean Code: A Handbook of Agile Software Craftsmanship", book.Title);
    }

    [Fact]
    public void IspCompliant_SelfCheckoutStation_CombinesSearchAndBorrowingOnly()
    {
        // ARRANGE: Trạm tự mượn trả kết hợp IBookSearchService và IBookBorrowingService
        var checkoutStation = new CleanSelfCheckoutStation();

        // ACT: Tra cứu và tự mượn sách
        var search = checkoutStation.SearchBooks("Refactoring");
        var borrowResult = checkoutStation.BorrowBook(3, "Hoàng Văn B");
        var returnResult = checkoutStation.ReturnBook(3, "Hoàng Văn B");

        // ASSERT
        Assert.Single(search);
        Assert.Contains("đã tự mượn cuốn", borrowResult);
        Assert.Contains("đã trả cuốn", returnResult);
    }

    [Fact]
    public void IspCompliant_LibrarianService_HandlesInventoryAndBarcodePrinting()
    {
        // ARRANGE: Dịch vụ thủ thư thực hiện quản trị kho và in ấn tem nhãn
        var librarianService = new CleanLibrarianInventoryService();

        // ACT: Thêm sách và in mã vạch
        var book = librarianService.AddNewBook("Microservices Patterns", "Chris Richardson", 450000m);
        var barcode = librarianService.PrintBarcode(book.Id, "TECH");
        bool priceUpdated = librarianService.UpdatePrice(book.Id, 480000m);
        int count = librarianService.AuditInventoryCount();

        // ASSERT
        Assert.NotNull(book);
        Assert.True(priceUpdated);
        Assert.Equal(1, count);
        Assert.StartsWith("BARCODE-[TECH]-BOOK#", barcode);
    }
}
