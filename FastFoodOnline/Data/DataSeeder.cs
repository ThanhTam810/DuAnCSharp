using FastFoodOnline.Models;
using Microsoft.AspNetCore.Identity;
using FastFoodOnline.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using static FastFoodOnline.Models.FoodItem;

namespace FastFoodOnline.Data
{
    public static class DataSeeder
    {
        // 1. Seed Roles bằng RoleManager
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleMgr)
        {
            foreach (var roleName in new[] { "Admin", "Customer" })
            {
                if (!await roleMgr.RoleExistsAsync(roleName))
                    await roleMgr.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        // 2. Seed Admin User bằng UserManager
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userMgr)
        {
            var adminEmail = "admin@fastfood.com";

            // 1. Tìm qua UserManager (đã dùng NormalizedEmail nội bộ)
            var admin = await userMgr.FindByEmailAsync(adminEmail);

            // 2. Fallback: query trực tiếp trên NormalizedEmail
            if (admin == null)
            {
                var normEmail = adminEmail.ToUpperInvariant();
                admin = userMgr.Users
                    .SingleOrDefault(u => u.NormalizedEmail == normEmail);
            }

            // 3. Nếu vẫn chưa có thì tạo mới
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Super Admin",
                    PhoneNumber = "0123456789",
                    Address = "Headquarters",
                    DateOfBirth = DateTime.Parse("1990-01-01")
                };

                await userMgr.CreateAsync(admin, "Admin@123");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }
        }



        // 3. Seed dữ liệu mẫu cho Category, FoodItem, Combo
        public static async Task SeedSampleDataAsync(FastFoodDbContext context)
        {
            // Category (loại bánh)
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Bánh ngọt" },
                    new Category { Name = "Đồ uống" }
                );
                await context.SaveChangesAsync();
            }

            // FoodItem (sản phẩm)
            if (!context.FoodItems.Any())
            {
                var sweetCat = context.Categories.First(c => c.Name == "Bánh ngọt");
                var drinkCat = context.Categories.First(c => c.Name == "Đồ uống");

                context.FoodItems.AddRange(
                    new FoodItem
                    {
                        Name = "Bánh Flan",
                        Description = "Bánh caramel mềm mịn, thơm ngon.",
                        Price = 2.99m,
                        CategoryId = sweetCat.Id,
                        ImageUrl = "/images/flan.jpg",
                        Status = ItemStatus.Available
                    },
                    new FoodItem
                    {
                        Name = "Bánh Su Kem",
                        Description = "Nhân kem béo ngậy, lớp vỏ vàng giòn.",
                        Price = 3.49m,
                        CategoryId = sweetCat.Id,
                        ImageUrl = "/images/sk.jpg",
                        Status = ItemStatus.Available
                    },
                    new FoodItem
                    {
                        Name = "Trà Sữa Trân Châu",
                        Description = "Trà sữa thơm béo kèm trân châu dẻo dai.",
                        Price = 2.50m,
                        CategoryId = drinkCat.Id,
                        ImageUrl = "/images/tstc.jpg",
                        Status = ItemStatus.Available
                    }
                );
                await context.SaveChangesAsync();
            }

            // Combo (set bánh + đồ uống)
            if (!context.Combos.Any())
            {
                var combo = new Combo
                {
                    Name = "Combo Bánh Flan + Trà Sữa",
                    Description = "Kết hợp ngọt ngào giữa bánh và trà sữa.",
                    Price = 4.99m
                };
                context.Combos.Add(combo);
                await context.SaveChangesAsync();

                var flan = context.FoodItems.First(f => f.Name == "Bánh Flan");
                var milkTea = context.FoodItems.First(f => f.Name == "Trà Sữa Trân Châu");

                context.ComboItems.AddRange(
                    new ComboItem { ComboId = combo.Id, FoodItemId = flan.Id, Quantity = 1 },
                    new ComboItem { ComboId = combo.Id, FoodItemId = milkTea.Id, Quantity = 1 }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
    
