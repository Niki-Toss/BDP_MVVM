#define MyAppName "BDP MVVM - Банк задач по программированию"
#define MyAppVersion "1.0"
#define MyAppPublisher "Пермский государственный национальный исследовательский университет"
#define MyAppPublisherShort "ПГНИУ"
#define MyAppFaculty "Институт компьютерных наук и технологий"
#define MyAppExeName "BDP_MVVM.exe"
#define MyAppURL "https://github.com/Niki-Toss/BDP_MVVM"

[Setup]
AppId={{E5B3C8D1-9A4F-4E2B-8C7D-1F6A9B3E4D5C}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppCopyright=© 2026 {#MyAppPublisherShort}

DefaultDirName={autopf}\BDP_MVVM
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
DisableProgramGroupPage=yes

OutputDir=C:\Users\Nikita\Source\repos\BDP_MVVM\InnoSetup_Output
OutputBaseFilename=BDP_MVVM_Setup_v{#MyAppVersion}

Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes
LZMANumBlockThreads=2

WizardStyle=modern

MinVersion=6.1sp1
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[CustomMessages]
russian.WelcomeLabel2=Это приложение установит [name/ver] на ваш компьютер.%n%nРазработано в рамках курсовой работы%nв {#MyAppPublisherShort}%n{#MyAppFaculty}

[Tasks]
Name: "desktopicon"; Description: "Создать ярлык на рабочем столе"; GroupDescription: "Дополнительные значки:"

[Files]
Source: "C:\Users\Nikita\Source\repos\BDP_MVVM\bin\Release\BDP_MVVM.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Nikita\Source\repos\BDP_MVVM\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Nikita\Source\repos\BDP_MVVM\bin\Release\*.config"; DestDir: "{app}"; Flags: ignoreversion

Source: "C:\Users\Nikita\Source\repos\BDP_MVVM\bin\Release\x86\*"; DestDir: "{app}\x86"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Users\Nikita\Source\repos\BDP_MVVM\bin\Release\x64\*"; DestDir: "{app}\x64"; Flags: ignoreversion recursesubdirs createallsubdirs

Source: "C:\Users\Nikita\Source\repos\BDP_MVVM\Redistributables\ndp472-kb4054530-x86-x64-allos-rus.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsDotNet472Installed

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "Банк задач по программированию MVVM"
Name: "{group}\Удалить {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{group}\О программе"; Filename: "{app}\{#MyAppExeName}"; Parameters: "--about"
Name: "{group}\GitHub проекта"; Filename: "{#MyAppURL}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "Банк задач по программированию"

[Run]
Filename: "{tmp}\ndp472-kb4054530-x86-x64-allos-rus.exe"; Parameters: "/q /norestart"; StatusMsg: "Установка .NET Framework 4.7.2..."; Flags: waituntilterminated; Check: not IsDotNet472Installed
Filename: "{app}\{#MyAppExeName}"; Description: "Запустить {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
function IsWindows7SP1OrHigher: Boolean;
var
  Version: TWindowsVersion;
begin
  GetWindowsVersionEx(Version);
  
  if (Version.Major = 6) and (Version.Minor = 1) then
  begin
    Result := Version.ServicePackMajor >= 1;
  end
  else if (Version.Major = 6) and (Version.Minor >= 2) then
    Result := True
  else if Version.Major >= 10 then
    Result := True
  else
    Result := False;
end;

function GetWindowsVersionString: String;
var
  Version: TWindowsVersion;
begin
  GetWindowsVersionEx(Version);
  Result := Format('Windows %d.%d SP%d', [Version.Major, Version.Minor, Version.ServicePackMajor]);
end;

function IsDotNet472Installed: Boolean;
var
  Release: Cardinal;
begin
  
  if RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) then
  begin
    Result := Release >= 461808;
    Log(Format('.NET Framework проверка: Release = %d (минимум 461808)', [Release]));
  end
  else
  begin
    Result := False;
    Log('.NET Framework не найден в реестре');
  end;
end;

function GetDotNetVersionString: String;
var
  Release: Cardinal;
begin
  if RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) then
  begin
    if Release >= 533320 then
      Result := '.NET Framework 4.8.1 или выше'
    else if Release >= 528040 then
      Result := '.NET Framework 4.8'
    else if Release >= 461808 then
      Result := '.NET Framework 4.7.2'
    else if Release >= 460798 then
      Result := '.NET Framework 4.7'
    else
      Result := Format('.NET Framework 4.x (Release: %d)', [Release]);
  end
  else
    Result := 'Не установлен';
end;

function InitializeSetup: Boolean;
var
  ErrorMessage: String;
  OsVersion: String;
  DotNetVersion: String;
begin
  OsVersion := GetWindowsVersionString;
  DotNetVersion := GetDotNetVersionString;
  
  Log('=== Проверка системы ===');
  Log('ОС: ' + OsVersion);
  Log('.NET: ' + DotNetVersion);

  if not IsWindows7SP1OrHigher then
  begin
    ErrorMessage := 'Для установки требуется Windows 7 SP1 или выше.' + #13#10#13#10 +
                    'Обнаружено: ' + OsVersion + #13#10#13#10 +
                    'Пожалуйста, установите Service Pack 1 для Windows 7 ' +
                    'или используйте более новую версию Windows.';
    MsgBox(ErrorMessage, mbError, MB_OK);
    Log('ОШИБКА: Неподдерживаемая версия Windows');
    Result := False;
    Exit;
  end;

  Log('✓ Версия Windows поддерживается');

  if not IsDotNet472Installed then
  begin
    Log('⚠ .NET Framework 4.7.2 не найден');
    
    ErrorMessage := '.NET Framework 4.7.2 не установлен.' + #13#10#13#10 +
                    'Обнаружено: ' + DotNetVersion + #13#10#13#10 +
                    'Для работы приложения требуется .NET Framework 4.7.2 или выше.' + #13#10#13#10 +
                    'Установщик автоматически установит .NET Framework 4.7.2.' + #13#10 +
                    'Это может занять несколько минут.' + #13#10#13#10 +
                    'Продолжить установку?';
    
    if MsgBox(ErrorMessage, mbConfirmation, MB_YESNO) = IDYES then
    begin
      Log('→ Пользователь согласился на установку .NET Framework');
      Result := True;
    end
    else
    begin
      Log('→ Пользователь отменил установку');
      MsgBox('Установка отменена. Для работы приложения требуется .NET Framework 4.7.2.', mbInformation, MB_OK);
      Result := False;
      Exit;
    end;
  end
  else
  begin
    Log('✓ .NET Framework 4.7.2+ уже установлен');
  end;

  Result := True;
  Log('=== Проверка завершена успешно ===');
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    Log('=== Установка завершена ===');
    Log('Путь установки: ' + ExpandConstant('{app}'));
    Log('База данных будет создана при первом запуске в:');
    Log(ExpandConstant('{userappdata}\BDP_MVVM\BDPData.db'));
  end;
end;

procedure InitializeWizard();
var
  InfoPage: TOutputMsgWizardPage;
begin
  InfoPage := CreateOutputMsgPage(wpWelcome,
    'Информация о программе',
    'BDP MVVM - Банк задач по программированию',
    'Разработано в рамках курсовой работы' + #13#10 + #13#10 +
    'Университет:' + #13#10 +
    'Пермский государственный национальный' + #13#10 +
    'исследовательский университет (ПГНИУ)' + #13#10 + #13#10 +
    'Факультет:' + #13#10 +
    'Институт компьютерных наук и технологий' + #13#10 + #13#10 +
    'GitHub: https://github.com/Niki-Toss/BDP_MVVM' + #13#10 + #13#10 +
    'Приложение использует:' + #13#10 +
    '• .NET Framework 4.7.2' + #13#10 +
    '• WPF (Windows Presentation Foundation)' + #13#10 +
    '• SQLite база данных' + #13#10 +
    '• Паттерн MVVM');
end;
