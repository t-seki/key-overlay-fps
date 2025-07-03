#!/bin/bash
# KeyOverlayFPS リリースビルドスクリプト（Linux/macOS用）
# このスクリプトはローカルでリリースパッケージを作成するためのものです

set -e

VERSION="${1:-1.0.0}"
ARCHITECTURES="${2:-x64,x86}"
OUTPUT_DIR="${3:-dist}"

echo "KeyOverlayFPS リリースビルド開始"
echo "バージョン: $VERSION"
echo "アーキテクチャ: $ARCHITECTURES"

# 出力ディレクトリのクリーンアップ
if [ -d "$OUTPUT_DIR" ]; then
    rm -rf "$OUTPUT_DIR"
fi
mkdir -p "$OUTPUT_DIR"

# プロジェクトルートに移動
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_ROOT"

echo "プロジェクトルート: $PROJECT_ROOT"

IFS=',' read -ra ARCH_ARRAY <<< "$ARCHITECTURES"
for arch in "${ARCH_ARRAY[@]}"; do
    echo ""
    echo "=== $arch ビルド開始 ==="
    
    # 単一実行ファイル版
    echo "単一実行ファイル版をビルド中..."
    single_output_path="$OUTPUT_DIR/single-$arch"
    
    dotnet publish src/KeyOverlayFPS.csproj \
        --configuration Release \
        --runtime "win-$arch" \
        --self-contained true \
        --output "$single_output_path" \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:EnableCompressionInSingleFile=true \
        -p:Version="$VERSION" \
        -p:AssemblyVersion="$VERSION.0" \
        -p:FileVersion="$VERSION.0"
    
    # 単一実行ファイルをコピー
    cp "$single_output_path/KeyOverlayFPS.exe" "$OUTPUT_DIR/KeyOverlayFPS-win-$arch.exe"
    echo "作成完了: $OUTPUT_DIR/KeyOverlayFPS-win-$arch.exe"
    
    # フォルダ版（Zip用）
    echo "フォルダ版をビルド中..."
    folder_output_path="$OUTPUT_DIR/folder-$arch"
    
    dotnet publish src/KeyOverlayFPS.csproj \
        --configuration Release \
        --runtime "win-$arch" \
        --self-contained true \
        --output "$folder_output_path" \
        -p:PublishSingleFile=false \
        -p:Version="$VERSION" \
        -p:AssemblyVersion="$VERSION.0" \
        -p:FileVersion="$VERSION.0"
    
    # レイアウトファイルをコピー
    if [ -d "layouts" ]; then
        cp -r layouts "$folder_output_path/"
        echo "レイアウトファイルをコピーしました"
    fi
    
    # README等をコピー（存在する場合）
    for readme in README.md LICENSE CHANGELOG.md; do
        if [ -f "$readme" ]; then
            cp "$readme" "$folder_output_path/"
            echo "ドキュメントをコピー: $readme"
        fi
    done
    
    # Zipファイルを作成
    cd "$folder_output_path"
    zip -r "../KeyOverlayFPS-win-$arch.zip" .
    cd "$PROJECT_ROOT"
    echo "作成完了: $OUTPUT_DIR/KeyOverlayFPS-win-$arch.zip"
    
    # 一時フォルダを削除
    rm -rf "$single_output_path" "$folder_output_path"
done

echo ""
echo "=== ビルド完了 ==="
echo "出力ディレクトリ: $OUTPUT_DIR"

# 作成されたファイルを表示
ls -lh "$OUTPUT_DIR"

echo ""
echo "リリース手順:"
echo "1. git tag v$VERSION"
echo "2. git push origin v$VERSION"
echo "3. GitHub Actionsでリリースが自動作成されます"