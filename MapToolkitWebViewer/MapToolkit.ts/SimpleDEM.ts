module SimpleDEM {

    abstract class DemDataCellBase {
        size: number[];
        pixelSize: number[];
        constructor(public start: number[], public end: number[], public pointsPerCell: number[], public data: ArrayLike<number>) {
            this.size = [
                end[0] - start[0],
                end[1] - start[1]
            ];
        }
        abstract getRawElevation(latLon: number[]): number;
    }

    class DemDataCellPixelIsPoint extends DemDataCellBase {
        constructor(start: number[], end: number[], pointsPerCell: number[], data: ArrayLike<number>) {
            super(start, end, pointsPerCell, data);
            this.pixelSize = [
                this.size[0] / (pointsPerCell[0] - 1),
                this.size[1] / (pointsPerCell[1] - 1),
            ];
        }
        getRawElevation(latLon: number[]): number {
            var relLat = Math.round((latLon[0] - this.start[0]) / this.size[0] * (this.pointsPerCell[0] - 1));
            var relLon = Math.round((latLon[1] - this.start[1]) / this.size[1] * (this.pointsPerCell[1] - 1));
            return this.data[relLat * this.pointsPerCell[1] + relLon];
        }
    }

    class DemDataCellPixelIsArea extends DemDataCellBase {
        constructor(start: number[], end: number[], pointsPerCell: number[], data: ArrayLike<number>) {
            super(start, end, pointsPerCell, data);
            this.pixelSize = [
                this.size[0] / pointsPerCell[0],
                this.size[1] / pointsPerCell[1],
            ];
        }
        getRawElevation(latLon: number[]): number {
            var relLat = Math.ceil((latLon[0] - this.start[0]) / this.size[0] * this.pointsPerCell[0]);
            var relLon = Math.ceil((latLon[1] - this.start[1]) / this.size[1] * this.pointsPerCell[1]);
            return this.data[relLat * this.pointsPerCell[1] + relLon];
        }
    }

    /**
     * Parse a DemDataCell from the specified ArrayBuffer
     * @param buffer Raw data
     */
    export function readDemDataCell(buffer: ArrayBuffer): DemDataCellBase {
        const header = new DataView(buffer, 0, 0x42);
        if (header.getUint32(0, true) != 0x57d15a3c) {
            throw new Error("Invalid file");
        }
        if (header.getUint8(0x4) != 0x01) {
            throw new Error("Invalid file");
        }
        const subVersion = header.getUint8(0x5);
        const dataType = header.getUint8(0x6);
        const rasterType = header.getUint8(0x7);
        const start = [header.getFloat64(0x8, true), header.getFloat64(0x10, true)];
        const end = [header.getFloat64(0x18, true), header.getFloat64(0x20, true)];
        const pointsPerCell = [header.getInt32(0x28, true), header.getInt32(0x2C, true)];
        const dataSize = header.getInt32(0x38, true);
        if (dataSize > buffer.byteLength + 0x42) {
            throw new Error("Invalid file");
        }
        var data: ArrayLike<number>;
        switch (dataType) { // Assumes little endian hardware
            case 0:
                data = new Float32Array(buffer, 0x42, dataSize);
                break;
            case 1:
                data = new Int16Array(buffer, 0x42, dataSize);
                break;
            case 2:
                data = new Uint16Array(buffer, 0x42, dataSize);
                break;
            case 3:
                data = new Float64Array(buffer, 0x42, dataSize);
                break;
            default:
                throw new Error("Invalid file");
        }
        if (rasterType == 2) {
            return new DemDataCellPixelIsPoint(start, end, pointsPerCell, data);
        }
        return new DemDataCellPixelIsArea(start, end, pointsPerCell, data);
    }


}