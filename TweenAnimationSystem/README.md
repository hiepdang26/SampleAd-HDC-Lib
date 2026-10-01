# Dependencies
- Odin: https://drive.google.com/file/d/1mp2TG89y8Vcj2wDIhiUduZ4wIFsBg7PQ/view?pli=1
- DOTween: https://drive.google.com/file/d/1P9AlXQIXp4YQ7WQuDOEx20FyUsqpCYoB/view?pli=1
# Usage (C#)
## TweenAnimation.cs
- Dùng làm Animation cho GameObject
## ScreenToggle.cs
- Thêm component vào GameObject Root
- Button Show All: Gọi tất cả Animation trong danh sách và Show
- Button Hide All: Gọi tất cả Animation trong danh sách và Hide
- Button Get All Child Animation: Tìm tất cả Child Object có TweenAnimation bỏ vào danh sách
## ScreenToggleManager.cs
- Gọi ScreenToggleManager.Toggle(bool) để tắt bật tất cả ScreenToggle đang có trên scene và được activate