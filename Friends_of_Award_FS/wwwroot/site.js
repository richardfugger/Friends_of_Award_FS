function downloadFile(filename, bytes) {
    const blob = new Blob([new Uint8Array(bytes)], {
        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    });

    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    link.click();
}