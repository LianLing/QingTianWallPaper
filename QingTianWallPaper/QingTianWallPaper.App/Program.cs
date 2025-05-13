// Program.cs
using Microsoft.Extensions.DependencyInjection;
using QingTianWallPaper.App;
using QingTianWallPaper.UI;
using QingTianWallPaper.UI.ViewModels;

var builder = App.CreateBuilder(args);

builder.Services.AddSingleton<MainWindow>();
// 注册服务和视图模型
builder.Services.AddTransient<MainWindowViewModel>();
// 其他服务注册...

var app = builder.Build();
app.Run();