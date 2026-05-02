# DemUtility (`dem`)

A command-line tool for managing Digital Elevation Model (DEM) databases produced or consumed by `Pmad.Cartography`.

## Installation

Build from source (requires .NET 10):

```
dotnet build DemUtility
```

Or publish a self-contained binary:

```
dotnet publish DemUtility -c Release -r win-x64 --self-contained
```

## Commands

### `repack` – Re-compress a DEM database

Creates a copy of a DEM database with a chosen compression format. Useful for converting downloaded archives (e.g. Zip) to faster formats such as ZSTD.

```
dem repack --source <dir> --target <dir> [options]
```

| Option | Short | Description | Default |
| ------ | ----- | ----------- | ------- |
| `--source` | `-s` | Source directory | *(required)* |
| `--target` | `-t` | Target directory | *(required)* |
| `--compression` | `-c` | `GZip`, `ZSTD`, `Brotli`, or `None` | `ZSTD` |
| `--max-cpu` | `-m` | Maximum CPU cores to use | all cores |
| `--keep` | `-k` | Skip files that already exist in target | `false` |

**Example**

```
dem repack -s ./raw-srtm -t ./srtm-zstd -c ZSTD
```

---

### `index` – Build a database index

Scans a local DEM directory and writes an `index.json` file listing all available cells. The index is required by `DemFileSystemStorage` for efficient tile lookup.

```
dem index --path <dir>
```

| Option | Short | Description |
| ------ | ----- | ----------- |
| `--path` | `-p` | Database directory |

**Example**

```
dem index -p ./srtm-zstd
```

---

### `update-index` – Add missing checksums to an existing index

Calculates and appends SHA-256 checksums for any cells that are listed in an existing `index.json` but do not yet have a checksum entry.

```
dem update-index --path <dir>
```

| Option | Short | Description |
| ------ | ----- | ----------- |
| `--path` | `-p` | Database directory |

---

### `check` – Verify a database

Reads a local or HTTP-hosted DEM database, reports its content and optionally verifies file integrity via SHA-256 checksums.

```
dem check (--path <dir> | --url <url>) [options]
```

| Option | Short | Description |
| ------ | ----- | ----------- |
| `--path` | `-p` | Local database directory |
| `--url` | `-u` | HTTP base URL of the database |
| `--verify` | `-v` | Verify SHA-256 checksums |
| `--sample` | `-n` | Number of cells to randomly sample (all cells when omitted) |

**Examples**

```
# Check a local database and verify all checksums
dem check -p ./srtm-zstd --verify

# Check a remote database and sample 100 random cells
dem check -u https://cdn.dem.pmad.net/SRTM1/ --verify --sample 100
```

## Supported compression formats

| Format | Extension | Notes |
| ------ | --------- | ----- |
| ZSTD   | `.zst`    | Recommended – best speed/size trade-off |
| GZip   | `.gz`     | Widest tool support |
| Brotli | `.br`     | Best compression ratio |
| None   | *(none)*  | Uncompressed |

## Dependencies

- [Pmad.Cartography](https://www.nuget.org/packages/Pmad.Cartography)
- [System.CommandLine](https://www.nuget.org/packages/System.CommandLine)
- [Pmad.ProgressTracking](https://www.nuget.org/packages/Pmad.ProgressTracking)
