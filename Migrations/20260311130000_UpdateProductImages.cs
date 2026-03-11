using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update product images to use reliable sources
            migrationBuilder.Sql(@"
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/headphones/400/300' WHERE Id = 1;
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/smartwatch/400/300' WHERE Id = 2;
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/laptop-stand/400/300' WHERE Id = 3;
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/keyboard/400/300' WHERE Id = 4;
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/usb-hub/400/300' WHERE Id = 5;
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/mouse/400/300' WHERE Id = 6;
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/monitor/400/300' WHERE Id = 7;
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/webcam/400/300' WHERE Id = 8;
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/lamp/400/300' WHERE Id = 9;
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/speaker/400/300' WHERE Id = 10;
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/phone-stand/400/300' WHERE Id = 11;
                UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/cables/400/300' WHERE Id = 12;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to original Unsplash URLs
            migrationBuilder.Sql(@"
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400' WHERE Id = 1;
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400' WHERE Id = 2;
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400' WHERE Id = 3;
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1511467687858-23d96c32e4ae?w=400' WHERE Id = 4;
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1610557892470-55d9e80c0bce?w=400' WHERE Id = 5;
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400' WHERE Id = 6;
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400' WHERE Id = 7;
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1587829741301-dc798b91a603?w=400' WHERE Id = 8;
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1507440658841-9a2dd3a70d17?w=400' WHERE Id = 9;
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400' WHERE Id = 10;
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1586953208448-b95a79798f07?w=400' WHERE Id = 11;
                UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1558002038-1091a1661116?w=400' WHERE Id = 12;
            ");
        }
    }
}
