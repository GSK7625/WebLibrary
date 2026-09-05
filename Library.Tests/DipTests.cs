using Library.Business.Dip;
using Library.Business.Legacy;
using Xunit;

namespace Library.Tests;

public class DipTests
{
    [Fact]
    public void DipViolation_BadNotificationManager_HasHardcodedDependencies()
    {
        // ARRANGE: Class vi phạm DIP tự new các module cấp thấp bên trong nó
        var badNotifier = new BadBorrowNotificationManager();

        // ACT
        var (logs, summary) = badNotifier.SendOverdueAlertBadDIP(
            "Trần Văn D",
            "0909123456",
            "tranvand@gmail.com",
            "Clean Architecture",
            7);

        // ASSERT: Hoạt động nhưng gắn chặt cứng với các class hạ tầng SMS/SMTP/File
        Assert.Equal(3, logs.Count);
        Assert.Contains(logs, l => l.Contains("SMS GATEWAY (HARDCODED)"));
        Assert.Contains(logs, l => l.Contains("SMTP MAILER (HARDCODED)"));
        Assert.Contains(logs, l => l.Contains("FILE LOG (HARDCODED)"));
    }

    [Fact]
    public async Task DipCompliant_BorrowNotificationService_OrchestratesViaAbstractions()
    {
        // ARRANGE: Cung cấp các adapter hiện thực INotificationSender và IAuditLogger (DIP)
        var senders = new List<INotificationSender>
        {
            new EmailNotificationSender(),
            new SmsNotificationSender(),
            new ZaloNotificationSender()
        };
        var auditLogger = new InMemoryAuditLogger();

        var notificationService = new BorrowNotificationApplicationService(senders, auditLogger);

        // ACT
        var response = await notificationService.SendOverdueNotificationAsync(
            borrowerName: "Đỗ Minh Khang",
            contactInfo: "khang.do@domain.vn / 0988776655",
            bookTitle: "Design Patterns",
            daysLate: 3);

        // ASSERT
        Assert.Equal(3, response.DeliveryResults.Count);
        Assert.All(response.DeliveryResults, r => Assert.True(r.IsSuccess));

        // Kiểm tra audit logs đã ghi nhận 3 sự kiện tương ứng
        Assert.Equal(3, response.AuditLogs.Count);
        Assert.Contains(response.AuditLogs, log => log.Contains("Email Channel"));
        Assert.Contains(response.AuditLogs, log => log.Contains("SMS Brandname Channel"));
        Assert.Contains(response.AuditLogs, log => log.Contains("Zalo Official Account"));
    }

    [Fact]
    public async Task DipCompliant_CanEasilyInjectMockNotificationSender_WithoutModifyingService()
    {
        // ARRANGE: Tạo Mock sender phục vụ Unit Test mà không cần gửi SMS/Email thật
        var mockSender = new MockCustomNotificationSender();
        var auditLogger = new InMemoryAuditLogger();

        var service = new BorrowNotificationApplicationService(new[] { mockSender }, auditLogger);

        // ACT
        var response = await service.SendOverdueNotificationAsync(
            "Nguyễn Thu Trang",
            "trang.nguyen@test.com",
            "Domain-Driven Design",
            10);

        // ASSERT: Dịch vụ nghiệp vụ hoạt động hoàn hảo với Mock Adapter
        Assert.Single(response.DeliveryResults);
        Assert.Equal("Mock Test Channel", response.DeliveryResults[0].Channel);
        Assert.True(mockSender.WasCalled);
        Assert.Equal(1, mockSender.CallCount);
    }

    private class MockCustomNotificationSender : INotificationSender
    {
        public string ChannelName => "Mock Test Channel";
        public bool WasCalled { get; private set; }
        public int CallCount { get; private set; }

        public Task<NotificationDeliveryResult> SendAsync(string recipient, string subject, string message)
        {
            WasCalled = true;
            CallCount++;
            return Task.FromResult(new NotificationDeliveryResult
            {
                IsSuccess = true,
                Channel = ChannelName,
                Recipient = recipient,
                DeliveryDetails = "Mock delivered without real network IO."
            });
        }
    }
}
