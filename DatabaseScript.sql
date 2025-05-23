USE [master]
GO
/****** Object:  Database [VTInformatica]    Script Date: 08/05/2025 19:36:00 ******/
CREATE DATABASE [VTInformatica]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'VTInformatica', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\VTInformatica.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'VTInformatica_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\VTInformatica_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [VTInformatica] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [VTInformatica].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [VTInformatica] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [VTInformatica] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [VTInformatica] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [VTInformatica] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [VTInformatica] SET ARITHABORT OFF 
GO
ALTER DATABASE [VTInformatica] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [VTInformatica] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [VTInformatica] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [VTInformatica] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [VTInformatica] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [VTInformatica] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [VTInformatica] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [VTInformatica] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [VTInformatica] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [VTInformatica] SET  ENABLE_BROKER 
GO
ALTER DATABASE [VTInformatica] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [VTInformatica] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [VTInformatica] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [VTInformatica] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [VTInformatica] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [VTInformatica] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [VTInformatica] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [VTInformatica] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [VTInformatica] SET  MULTI_USER 
GO
ALTER DATABASE [VTInformatica] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [VTInformatica] SET DB_CHAINING OFF 
GO
ALTER DATABASE [VTInformatica] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [VTInformatica] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [VTInformatica] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [VTInformatica] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [VTInformatica] SET QUERY_STORE = ON
GO
ALTER DATABASE [VTInformatica] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [VTInformatica]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](450) NOT NULL,
	[ProviderKey] [nvarchar](450) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[FirstName] [nvarchar](max) NOT NULL,
	[LastName] [nvarchar](max) NOT NULL,
	[BirthDate] [date] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CartItems]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CartItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CartId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_CartItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Carts]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Carts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[TotalPrice] [decimal](18, 2) NULL,
	[Email] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Carts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Manufacturers]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Manufacturers](
	[ManufacturerId] [int] IDENTITY(1,1) NOT NULL,
	[ManufacturerName] [nvarchar](max) NOT NULL,
	[ManufacturerLogo] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Manufacturers] PRIMARY KEY CLUSTERED 
(
	[ManufacturerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItem]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_OrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNumber] [nvarchar](max) NOT NULL,
	[CustomerId] [nvarchar](450) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CustomerComment] [nvarchar](max) NULL,
	[IsDeleted] [bit] NOT NULL,
	[CustomerEmail] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductImages]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductImages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ImageUrl] [nvarchar](max) NOT NULL,
	[ProductId] [int] NOT NULL,
 CONSTRAINT [PK_ProductImages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ManufacturerId] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Quantity] [int] NOT NULL,
	[FullDescription] [nvarchar](max) NULL,
	[CategoryId] [int] NOT NULL,
	[SubCategoryId] [int] NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reviews]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reviews](
	[ReviewId] [int] IDENTITY(1,1) NOT NULL,
	[ReviewRating] [int] NOT NULL,
	[ReviewComment] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ProductId] [int] NOT NULL,
	[Email] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Reviews] PRIMARY KEY CLUSTERED 
(
	[ReviewId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SubCategories]    Script Date: 08/05/2025 19:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubCategories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[CategoryId] [int] NOT NULL,
 CONSTRAINT [PK_SubCategories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250409142601_VTInformaticaDB', N'8.0.14')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250409154309_updateManufacturer', N'8.0.14')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250410093707_AddReviews+', N'8.0.14')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250410135143_modifiedOrder', N'8.0.14')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250410143916_modifiedOrders2', N'8.0.14')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250410161830_addedTotalToCart', N'8.0.14')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250411092112_modifiedGetByIdToEmail', N'8.0.14')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250411092233_modifiedGetByIdToEmail2', N'8.0.14')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250508102202_Update', N'8.0.14')
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'98b4e551-a53d-40fc-9fa4-29d6dd582481', N'Admin', N'ADMIN', N'98b4e551-a53d-40fc-9fa4-29d6dd582481')
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'9ab947f2-b737-46bf-a388-2f8962573a2b', N'Seller', N'SELLER', N'9ab947f2-b737-46bf-a388-2f8962573a2b')
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'bd7ef491-0f1b-4b8c-9c80-340e14cfac9b', N'Customer', N'CUSTOMER', N'bd7ef491-0f1b-4b8c-9c80-340e14cfac9b')
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId], [CreatedAt]) VALUES (N'1692ce2c-e1f0-478a-b8c5-5fad1447cfa9', N'98b4e551-a53d-40fc-9fa4-29d6dd582481', CAST(N'2025-05-08T12:34:51.1333333' AS DateTime2))
GO
INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [BirthDate], [IsActive], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'1692ce2c-e1f0-478a-b8c5-5fad1447cfa9', N'Vittorio', N'Turiaci', CAST(N'2000-07-09' AS Date), 1, N'user@gmail.com', N'USER@GMAIL.COM', N'user@gmail.com', N'USER@GMAIL.COM', 0, N'AQAAAAIAAYagAAAAECu9h09ULDZ8NdZizbOGl73OYacWj7BUF7UDQFczfm2XItMWy7A0fidlz6okxLgTuQ==', N'OT4SBR7CXK3AUGXBFMQJQI5S4COUCFO6', N'c9e0deff-dd4d-436c-9482-3c27aa6142a8', NULL, 0, 0, NULL, 1, 0)
GO
SET IDENTITY_INSERT [dbo].[Carts] ON 

INSERT [dbo].[Carts] ([Id], [UserId], [CreatedAt], [TotalPrice], [Email]) VALUES (8, N'1692ce2c-e1f0-478a-b8c5-5fad1447cfa9', CAST(N'2025-05-08T10:44:01.1387027' AS DateTime2), CAST(2975.95 AS Decimal(18, 2)), N'user@gmail.com')
SET IDENTITY_INSERT [dbo].[Carts] OFF
GO
SET IDENTITY_INSERT [dbo].[Categories] ON 

INSERT [dbo].[Categories] ([Id], [Name]) VALUES (1, N'Componenti PC')
INSERT [dbo].[Categories] ([Id], [Name]) VALUES (2, N'Perferiche')
INSERT [dbo].[Categories] ([Id], [Name]) VALUES (3, N'Gaming')
INSERT [dbo].[Categories] ([Id], [Name]) VALUES (4, N'Accessori e Cavi')
INSERT [dbo].[Categories] ([Id], [Name]) VALUES (5, N'Networking')
INSERT [dbo].[Categories] ([Id], [Name]) VALUES (6, N'PC Assemblati')
SET IDENTITY_INSERT [dbo].[Categories] OFF
GO
SET IDENTITY_INSERT [dbo].[Manufacturers] ON 

INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (1, N'Intel', N'https://cdn.worldvectorlogo.com/logos/intel.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (2, N'AMD', N'https://cdn.worldvectorlogo.com/logos/amd-logo-1.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (3, N'Nvidia', N'https://cdn.worldvectorlogo.com/logos/nvidia.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (4, N'ASUS', N'https://cdn.worldvectorlogo.com/logos/asus-logo.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (5, N'Corsair', N'https://cdn.worldvectorlogo.com/logos/corsair-2.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (6, N'MSI', N'https://cdn.worldvectorlogo.com/logos/micro-star-international-logo.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (7, N'Gigabyte', N'https://cdn.worldvectorlogo.com/logos/gigabyte-technology-logo-2008.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (8, N'ThermalRight', N'https://upload.wikimedia.org/wikipedia/commons/3/34/Thermalright_Logo.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (9, N'Be quiet!', N'https://cdn.worldvectorlogo.com/logos/be-quiet-logo.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (10, N'LG', N'https://cdn.worldvectorlogo.com/logos/lg-electronics.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (11, N'Noctua', N'https://noctua.at/pub/static/version1743603791/frontend/SIT/noctua/en_US/images/logo.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (12, N'Samsung', N'https://cdn.worldvectorlogo.com/logos/samsung-8.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (13, N'Seagate', N'https://cdn.worldvectorlogo.com/logos/seagate-logo-1.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (14, N'Western Digital', N'https://cdn.worldvectorlogo.com/logos/western-digital-2.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (15, N'Kingston', N'https://cdn.worldvectorlogo.com/logos/kingston-1.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (16, N'Crucial', N'https://cdn.worldvectorlogo.com/logos/crucial.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (17, N'EVGA', N'https://cdn.worldvectorlogo.com/logos/evga.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (18, N'Cooler Master', N'https://cdn.worldvectorlogo.com/logos/cooler-master-black-logo.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (19, N'Thermaltake', N'https://cdn.worldvectorlogo.com/logos/thermaltake.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (20, N'Razer', N'https://cdn.worldvectorlogo.com/logos/razer-1.svg')
INSERT [dbo].[Manufacturers] ([ManufacturerId], [ManufacturerName], [ManufacturerLogo]) VALUES (21, N'NZXT', N'https://cdn.worldvectorlogo.com/logos/nzxt-1.svg')
SET IDENTITY_INSERT [dbo].[Manufacturers] OFF
GO
SET IDENTITY_INSERT [dbo].[OrderItem] ON 

INSERT [dbo].[OrderItem] ([Id], [OrderId], [ProductId], [Quantity]) VALUES (15, 11, 11, 1)
SET IDENTITY_INSERT [dbo].[OrderItem] OFF
GO
SET IDENTITY_INSERT [dbo].[Orders] ON 

INSERT [dbo].[Orders] ([Id], [OrderNumber], [CustomerId], [CreatedAt], [CustomerComment], [IsDeleted], [CustomerEmail]) VALUES (11, N'691A3022', N'1692ce2c-e1f0-478a-b8c5-5fad1447cfa9', CAST(N'2025-05-08T10:44:04.4474017' AS DateTime2), N'Ordine creato dal carrello', 1, N'user@gmail.com')
SET IDENTITY_INSERT [dbo].[Orders] OFF
GO
SET IDENTITY_INSERT [dbo].[ProductImages] ON 

INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (5, N'https://media.ldlc.com/r374/ld/products/00/06/20/52/LD0006205243.jpg', 3)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (6, N'https://media.ldlc.com/r374/ld/products/00/06/20/52/LD0006205244.jpg', 3)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (7, N'https://media.ldlc.com/r374/ld/products/00/06/20/52/LD0006205245.jpg', 3)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (8, N'https://media.ldlc.com/r374/ld/products/00/06/20/52/LD0006205246.jpg', 3)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (9, N'https://media.ldlc.com/r374/ld/products/00/06/20/52/LD0006205247.jpg', 3)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (10, N'https://media.ldlc.com/r374/ld/products/00/06/06/32/LD0006063225.jpg', 4)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (11, N'https://media.ldlc.com/r374/ld/products/00/06/06/32/LD0006063226.jpg', 4)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (12, N'https://media.ldlc.com/r374/ld/products/00/06/06/32/LD0006063228.jpg', 4)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (13, N'https://media.ldlc.com/r374/ld/products/00/06/06/32/LD0006063230.jpg', 4)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (14, N'https://media.ldlc.com/r374/ld/products/00/06/06/32/LD0006063232.jpg', 4)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (24, N'https://media.ldlc.com/r1600/ld/products/00/06/20/93/LD0006209366.jpg', 6)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (25, N'https://media.ldlc.com/r1600/ld/products/00/06/20/93/LD0006209365.jpg', 6)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (26, N'https://media.ldlc.com/r1600/ld/products/00/06/20/93/LD0006209367.jpg', 6)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (30, N'https://media.ldlc.com/r374/ld/products/00/05/96/95/LD0005969567_0005969592_0005969595_0005969598_0005969628_0006027330.jpg', 7)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (31, N'https://media.ldlc.com/r374/ld/products/00/05/96/95/LD0005969590_0005969593_0005969596_0005969599_0005969629_0006027331.jpg', 7)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (32, N'https://media.ldlc.com/r374/ld/products/00/05/96/95/LD0005969591_0005969594_0005969597_0005969600_0005969630_0006027332.jpg', 7)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (33, N'https://media.ldlc.com/r1600/ld/products/00/06/19/16/LD0006191687.jpg', 8)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (34, N'https://media.ldlc.com/r1600/ld/products/00/06/19/16/LD0006191688.jpg', 8)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (35, N'https://media.ldlc.com/r1600/ld/products/00/06/19/16/LD0006191689.jpg', 8)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (36, N'https://media.ldlc.com/r1600/ld/products/00/06/19/16/LD0006191690.jpg', 8)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (37, N'https://media.ldlc.com/r1600/ld/products/00/06/19/16/LD0006191691.jpg', 8)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (38, N'https://media.ldlc.com/r1600/ld/products/00/05/73/30/LD0005733043_1.jpg', 9)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (39, N'https://media.ldlc.com/r1600/ld/products/00/05/73/30/LD0005733045_1.jpg', 9)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (40, N'https://media.ldlc.com/r1600/ld/products/00/05/73/30/LD0005733044_1.jpg', 9)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (41, N'https://media.ldlc.com/r1600/ld/products/00/05/73/29/LD0005732992_1_0005733040.jpg', 9)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (42, N'https://media.ldlc.com/r1600/ld/products/00/05/73/29/LD0005732991_1_0005733039.jpg', 9)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (48, N'https://media.ldlc.com/r1600/ld/products/00/06/12/14/LD0006121499.jpg', 11)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (49, N'https://media.ldlc.com/r1600/ld/products/00/06/12/15/LD0006121500.jpg', 11)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (50, N'https://media.ldlc.com/r1600/ld/products/00/06/12/15/LD0006121501.jpg', 11)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (51, N'https://media.ldlc.com/r1600/ld/products/00/06/12/15/LD0006121503.jpg', 11)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (52, N'https://media.ldlc.com/r1600/ld/products/00/06/12/15/LD0006121504.jpg', 11)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (53, N'https://media.ldlc.com/r1600/ld/products/00/06/01/33/LD0006013385.jpg', 12)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (54, N'https://media.ldlc.com/r1600/ld/products/00/06/01/33/LD0006013386.jpg', 12)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (55, N'https://media.ldlc.com/r1600/ld/products/00/06/01/33/LD0006013387.jpg', 12)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (63, N'https://media.ldlc.com/r374/ld/products/00/06/11/78/LD0006117861.jpg', 15)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (64, N'https://media.ldlc.com/r374/ld/products/00/05/90/26/LD0005902625_1.jpg', 16)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (65, N'https://media.ldlc.com/r1600/ld/products/00/06/18/10/LD0006181036.jpg', 17)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (66, N'https://media.ldlc.com/r1600/ld/products/00/06/18/10/LD0006181037.jpg', 17)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (67, N'https://media.ldlc.com/r1600/ld/products/00/06/18/10/LD0006181038.jpg', 17)
INSERT [dbo].[ProductImages] ([Id], [ImageUrl], [ProductId]) VALUES (68, N'https://media.ldlc.com/r374/ld/products/00/06/07/49/LD0006074923.jpg', 18)
SET IDENTITY_INSERT [dbo].[ProductImages] OFF
GO
SET IDENTITY_INSERT [dbo].[Products] ON 

INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (3, 6, N'MSI GeForce RTX 5090 32G GAMING TRIO OC.', N'32 GB GDDR7 - HDMI/Tri DisplayPort - DLSS 4 - PCI Express (NVIDIA GeForce RTX 5090).', CAST(2975.95 AS Decimal(18, 2)), 10, N'Sfrutta le rivoluzionarie capacità creative e di gioco con la fenomenale potenza della GPU NVIDIA GeForce RTX 5090. Gioca in 4K 480Hz o 8K 120Hz con la GPU di gioco più potente di sempre.', 1, 4)
INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (4, 6, N'MSI Z790 GAMING PLUS WIFI', N'Scheda madre ATX Socket 1700 Intel Z790 Express - 4x DDR5 - M.2 PCIe 4.0 - USB 3.2 - PCI-Express 5.0 16x - LAN 2.5 GbE - Wi-Fi 6E/Bluetooth 5.3', CAST(230.95 AS Decimal(18, 2)), 17, N'Progettata per ospitare i processori ibridi Intel Core di 12a e 13a generazione su socket Intel LGA 1700, la scheda madre MSI Z790 GAMING PLUS WIFI è ideale per assemblare una configurazione Gaming. La Z790 GAMING PLUS WIFI offre anche una gamma completa di opzioni di connettività.', 1, 2)
INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (6, 12, N'Samsung SSD 990 EVO Plus M.2 PCIe NVMe 1Tb', N'SSD 1Tb M.2 2280 NVMe 2.0 - PCIe 5.0 x2/PCIe 4.0 x4.', CAST(78.95 AS Decimal(18, 2)), 30, N'L''SSD 990 EVO Plus 1TB di Samsung offre un aumento delle prestazioni al tuo computer per ridurre i tempi di caricamento e trasferire velocemente file di grandi dimensioni. Queste prestazioni sono rese possibili da una velocità di lettura fino a 7150 MB/s.', 1, 5)
INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (7, 5, N'Corsair Vengeance RGB DDR5 32 GB (2 x 16 GB) 6000 MHz CL36 - Nero', N'Kit a doppio canale 2 array di RAM PC5-48000 DDR5 RGB - CMH32GX5M2E6000C36', CAST(121.95 AS Decimal(18, 2)), 10, N'La memoria Corsair Vengeance RGB DDR5 offre prestazioni e frequenze più elevate con migliori funzionalità ottimizzate per le schede madri Intel e illumina il tuo PC con un''illuminazione RGB dinamica in dieci zone personalizzabili.', 1, 3)
INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (8, 20, N'Razer Katana Chroma 850W .', N'PSU modulare 850W ATX12V - Ventola 140mm - Razer Chroma aRGB - 80 PLUS Platinum .', CAST(127.95 AS Decimal(18, 2)), 8, N'L''alimentatore Razer Katana Chroma è 100% modulare e certificato 80+ Platinum. Ultra efficiente, Katana Chroma è progettato per supportare i processori e le schede grafiche più potenti del mercato.', 1, 6)
INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (9, 5, N'Corsair 4000D AIRFLOW Vetro temperato (bianco)', N'Case PC a torre media con pannello in vetro temperato e pannello frontale MESH', CAST(84.95 AS Decimal(18, 2)), 8, N'Il Corsair 4000D è un superbo case per PC mid-tower. È sobrio, efficiente e vanta un sublime design minimalista. In grado di ospitare i componenti più potenti, offre eccellenti prestazioni di raffreddamento.', 1, 7)
INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (11, 10, N'LED LG 44,5" - 45GR75DC-B.', N'Display PC 5K - 5120 x 1440 pixel - 1 ms (scala di grigi) - 32/9 - Pannello VA curvo - 200 Hz - DisplayHDR 600 - FreeSync Premium Pro - HDMI/DisplayPort/USB-C - Altezza regolabile - Nero', CAST(897.95 AS Decimal(18, 2)), 4, N'Con il suo ampio display da 44,5 pollici in formato 32:9, il monitor LG 45GR75DC-B è progettato per offrire il massimo del comfort e delle prestazioni. Questo schermo LG ha tutto quello che serve: pannello VA curvo 1500R, frequenza di 200 Hz, tecnologia FreeSync Premium Pro e certificazione DisplayHDR 600.', 2, 9)
INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (12, 4, N'ASUS ROG Chakram X Origine.', N'Mouse cablato o wireless per giocatori - destrimano - sensore ottico da 36000 DPI - 11 pulsanti - joystick programmabile - caricamento rapido - modulare - retroilluminazione RGB', CAST(127.95 AS Decimal(18, 2)), 8, N'Con il mouse ASUS ROG Chakram X Origin entri in una nuova era. Grazie alla personalizzazione avanzata, al sensore ottico di fascia alta da 36.000 dpi e all''innovativo sistema di interruttori intercambiabili, è la tua migliore scommessa per vincere molte battaglie.', 2, 11)
INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (15, 20, N'Razer Seiren v3 Mini (nero)', N'Microfono USB - ultracompatto - supercardioide - funzione Tap-to-Mute - indicatore LED', CAST(352.95 AS Decimal(18, 2)), 5, N'Il Razer Seiren v3 Mini è un microfono a condensatore ultracompatto che rappresenta la soluzione ideale per ottenere un suono di qualità professionale in qualsiasi configurazione broadcast..', 2, 13)
INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (16, 4, N'ASUS ROG Eye S', N'Webcam - Full HD 1080p 60 fps - microfono con cancellazione del rumore - auto focus - pieghevole', CAST(79.95 AS Decimal(18, 2)), 5, N'Porta il tuo equipaggiamento di trasmissione al livello successivo con la webcam ASUS ROG Eye S, che è stata appositamente progettata per soddisfare le esigenze dei giocatori e degli streamer. Capace di catturare un flusso video a 1080p a 60 fps costanti, ha anche un microfono con cancellazione del rumore.', 2, 14)
INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (17, 2, N'AMD Ryzen 7 9800X3D (4,7 GHz / 5,2 GHz).', N'AMD Processore 8-Core 16-Threads socket AM5 3D V-Cache 104 Mo 4 nm TDP 120W (versione box senza ventola - Garanzia del produttore di 3 anni)', CAST(608.95 AS Decimal(18, 2)), 3, N'AMD Il processore Ryzen 7 9800X3D basato sull''architettura Zen 5 e dotato di tecnologia 3D V-Cache ti permette di raggiungere nuove vette. Grazie a frame rate più fluidi e a prestazioni spettacolari, questo processore ti garantisce un gioco senza problemi, gioco dopo gioco.', 1, 1)
INSERT [dbo].[Products] ([Id], [ManufacturerId], [Name], [Description], [Price], [Quantity], [FullDescription], [CategoryId], [SubCategoryId]) VALUES (18, 1, N'    Intel Core i9-14900K (3,2 GHz / 5,8 GHz)', N'Processore 24-Core (8 Performance-Cores + 16 Efficient-Cores) 32-Threads Socket 1700 Cache L3 36 MB Intel UHD Graphics 770 0,010 micron (versione senza ventola - garanzia Intel di 3 anni)', CAST(530.95 AS Decimal(18, 2)), 14, N'Con più core e frequenze più elevate, i processori Intel Core di 14a generazione ti permettono di fare di più, senza compromettere le prestazioni. Sono progettati per soddisfare tutte le tue esigenze e per offrirti la migliore esperienza di gioco possibile.', 1, 1)
SET IDENTITY_INSERT [dbo].[Products] OFF
GO
SET IDENTITY_INSERT [dbo].[SubCategories] ON 

INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (1, N'CPU', 1)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (2, N'Schede madri', 1)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (3, N'RAMs', 1)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (4, N'Schede video', 1)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (5, N'SSD / Hard Disk', 1)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (6, N'Alimentatori', 1)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (7, N'Case', 1)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (8, N'Dissipatori', 1)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (9, N'Monitor', 2)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (10, N'Tastiere', 2)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (11, N'Mouse', 2)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (12, N'Cuffie', 2)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (13, N'Microfoni', 2)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (14, N'Webcam', 2)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (15, N'Controller', 3)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (16, N'Volanti', 3)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (17, N'Sedie da gaming', 3)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (18, N'Accessori RGB', 3)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (19, N'Cavi di alimentazione', 4)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (20, N'Adattatori e hub', 4)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (21, N'Ventole', 4)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (22, N'Paste termiche', 4)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (23, N'Router', 5)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (24, N'Schede di rete', 5)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (25, N'Range extender', 5)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (26, N'Gaming entry-level', 6)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (27, N'Gaming mid-range', 6)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (28, N'Gaming high-end', 6)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (29, N'Workstation', 6)
INSERT [dbo].[SubCategories] ([Id], [Name], [CategoryId]) VALUES (35, N'Mini PC', 6)
SET IDENTITY_INSERT [dbo].[SubCategories] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 08/05/2025 19:36:00 ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[NormalizedName] ASC
)
WHERE ([NormalizedName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [EmailIndex]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 08/05/2025 19:36:00 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CartItems_CartId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_CartItems_CartId] ON [dbo].[CartItems]
(
	[CartId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CartItems_ProductId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_CartItems_ProductId] ON [dbo].[CartItems]
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Carts_UserId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Carts_UserId] ON [dbo].[Carts]
(
	[UserId] ASC
)
WHERE ([UserId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderItem_OrderId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_OrderItem_OrderId] ON [dbo].[OrderItem]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderItem_ProductId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_OrderItem_ProductId] ON [dbo].[OrderItem]
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Orders_CustomerId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_Orders_CustomerId] ON [dbo].[Orders]
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductImages_ProductId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_ProductImages_ProductId] ON [dbo].[ProductImages]
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_CategoryId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_Products_CategoryId] ON [dbo].[Products]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_ManufacturerId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_Products_ManufacturerId] ON [dbo].[Products]
(
	[ManufacturerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_SubCategoryId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_Products_SubCategoryId] ON [dbo].[Products]
(
	[SubCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Reviews_ProductId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_Reviews_ProductId] ON [dbo].[Reviews]
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Reviews_UserId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_Reviews_UserId] ON [dbo].[Reviews]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SubCategories_CategoryId]    Script Date: 08/05/2025 19:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_SubCategories_CategoryId] ON [dbo].[SubCategories]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AspNetUserRoles] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Carts] ADD  DEFAULT (N'') FOR [Email]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (N'') FOR [CustomerEmail]
GO
ALTER TABLE [dbo].[Reviews] ADD  DEFAULT (N'') FOR [Email]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD  CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY([CartId])
REFERENCES [dbo].[Carts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CartItems] CHECK CONSTRAINT [FK_CartItems_Carts_CartId]
GO
ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD  CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CartItems] CHECK CONSTRAINT [FK_CartItems_Products_ProductId]
GO
ALTER TABLE [dbo].[Carts]  WITH CHECK ADD  CONSTRAINT [FK_Carts_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Carts] CHECK CONSTRAINT [FK_Carts_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD  CONSTRAINT [FK_OrderItem_Orders_OrderId] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderItem] CHECK CONSTRAINT [FK_OrderItem_Orders_OrderId]
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD  CONSTRAINT [FK_OrderItem_Products_ProductId] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderItem] CHECK CONSTRAINT [FK_OrderItem_Products_ProductId]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_AspNetUsers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_AspNetUsers_CustomerId]
GO
ALTER TABLE [dbo].[ProductImages]  WITH CHECK ADD  CONSTRAINT [FK_ProductImages_Products_ProductId] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProductImages] CHECK CONSTRAINT [FK_ProductImages_Products_ProductId]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_Categories_CategoryId]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_Manufacturers_ManufacturerId] FOREIGN KEY([ManufacturerId])
REFERENCES [dbo].[Manufacturers] ([ManufacturerId])
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_Manufacturers_ManufacturerId]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_SubCategories_SubCategoryId] FOREIGN KEY([SubCategoryId])
REFERENCES [dbo].[SubCategories] ([Id])
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_SubCategories_SubCategoryId]
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [FK_Reviews_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [FK_Reviews_Products_ProductId] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_Products_ProductId]
GO
ALTER TABLE [dbo].[SubCategories]  WITH CHECK ADD  CONSTRAINT [FK_SubCategories_Categories_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([Id])
GO
ALTER TABLE [dbo].[SubCategories] CHECK CONSTRAINT [FK_SubCategories_Categories_CategoryId]
GO
USE [master]
GO
ALTER DATABASE [VTInformatica] SET  READ_WRITE 
GO
