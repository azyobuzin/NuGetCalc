#
# モジュール 'NuGetCalc' のモジュール マニフェスト
#
# 生成者: azyobuzin
#
# 生成日: 2015/03/06
#

@{

# このマニフェストに関連付けられているスクリプト モジュール ファイルまたはバイナリ モジュール ファイル。
# RootModule = ''

# このモジュールのバージョン番号です。
ModuleVersion = '1.3'

# このモジュールを一意に識別するために使用される ID
GUID = '9a7d0b4c-fa66-48f7-aea8-b7ede6025a2a'

# このモジュールの作成者
Author = 'azyobuzin'

# このモジュールの会社またはベンダー
CompanyName = 'azyobuzin'

# このモジュールの著作権情報
Copyright = '(c) 2014-2015 azyobuzin'

# このモジュールの機能の説明
# Description = ''

# このモジュールに必要な Windows PowerShell エンジンの最小バージョン
# PowerShellVersion = ''

# このモジュールに必要な Windows PowerShell ホストの名前
# PowerShellHostName = ''

# このモジュールに必要な Windows PowerShell ホストの最小バージョン
# PowerShellHostVersion = ''

# このモジュールに必要な Microsoft .NET Framework の最小バージョン
# DotNetFrameworkVersion = ''

# このモジュールに必要な共通言語ランタイム (CLR) の最小バージョン
# CLRVersion = ''

# このモジュールに必要なプロセッサ アーキテクチャ (なし、X86、Amd64)
# ProcessorArchitecture = ''

# このモジュールをインポートする前にグローバル環境にインポートされている必要があるモジュール
# RequiredModules = @()

# このモジュールをインポートする前に読み込まれている必要があるアセンブリ
RequiredAssemblies = @(
    'System.IO.Compression',
    'System.IO.Compression.FileSystem'
)

# このモジュールをインポートする前に呼び出し元の環境で実行されるスクリプト ファイル (.ps1)。
# ScriptsToProcess = @()

# このモジュールをインポートするときに読み込まれる型ファイル (.ps1xml)
# TypesToProcess = @()

# このモジュールをインポートするときに読み込まれる書式ファイル (.ps1xml)
# FormatsToProcess = @()

# RootModule/ModuleToProcess に指定されているモジュールの入れ子になったモジュールとしてインポートするモジュール
NestedModules = @('.\NuGetCalc.psm1')

# このモジュールからエクスポートする関数
FunctionsToExport = '*'

# このモジュールからエクスポートするコマンドレット
CmdletsToExport = '*'

# このモジュールからエクスポートする変数
VariablesToExport = '*'

# このモジュールからエクスポートするエイリアス
AliasesToExport = '*'

# このモジュールに同梱されているすべてのモジュールのリスト
# ModuleList = @()

# このモジュールに同梱されているすべてのファイルのリスト
# FileList = @()

# RootModule/ModuleToProcess に指定されているモジュールに渡すプライベート データ
# PrivateData = ''

# このモジュールの HelpInfo URI
# HelpInfoURI = ''

# このモジュールからエクスポートされたコマンドの既定のプレフィックス。既定のプレフィックスをオーバーライドする場合は、Import-Module -Prefix を使用します。
# DefaultCommandPrefix = ''

}

