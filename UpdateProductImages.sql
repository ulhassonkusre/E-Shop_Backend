-- Update product images to use reliable Picsum Photos service
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/headphones/400/300' WHERE Id = 1;
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/smartwatch/400/300' WHERE Id = 2;
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/laptop/400/300' WHERE Id = 3;
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/keyboard/400/300' WHERE Id = 4;
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/usb/400/300' WHERE Id = 5;
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/mouse/400/300' WHERE Id = 6;
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/screen/400/300' WHERE Id = 7;
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/camera/400/300' WHERE Id = 8;
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/light/400/300' WHERE Id = 9;
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/audio/400/300' WHERE Id = 10;
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/mobile/400/300' WHERE Id = 11;
UPDATE Products SET ImageUrl = 'https://picsum.photos/seed/tech/400/300' WHERE Id = 12;

-- Verify updates
SELECT Id, Name, ImageUrl FROM Products;
