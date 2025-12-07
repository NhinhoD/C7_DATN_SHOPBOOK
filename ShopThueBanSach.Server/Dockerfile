# -----------------------------
# STEP 1: Build the application
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ShopThueBanSach.sln ./
COPY ShopThueBanSach.Server/ ShopThueBanSach.Server/
# Lưu ý: Nếu bạn có project client nằm trong solution, hãy copy luôn file csproj của nó nếu cần thiết
# COPY ShopThueBanSach.Client/ ShopThueBanSach.Client/ 

# Restore NuGet packages
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Publish the application
RUN dotnet publish ShopThueBanSach.Server/ShopThueBanSach.Server.csproj -c Release -o /app/publish

# -----------------------------
# STEP 2: Runtime image (QUAN TRỌNG)
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# --- CẤU HÌNH FIX LỖI TIMEOUT TRÊN RENDER ---

# 1. Tắt IPv6: Buộc .NET chỉ dùng IPv4. Đây là chìa khóa để fix lỗi kết nối Gmail trên Render.
ENV DOTNET_SYSTEM_NET_DISABLEIPV6=1

# 2. Cấu hình Port: .NET 8 trong Docker mặc định chạy port 8080.
# Render sẽ set biến PORT, dòng này để đảm bảo app lắng nghe trên mọi IP
ENV ASPNETCORE_URLS=http://+:8080

# Expose port (Chỉ mang tính tài liệu, Render sẽ tự map port)
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "ShopThueBanSach.Server.dll"]