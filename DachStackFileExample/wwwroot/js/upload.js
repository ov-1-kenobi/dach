document.addEventListener('DOMContentLoaded', function () {
    const uploadButton = document.getElementById('uploadButton');
    const fileInput = document.getElementById('fileInput');
    const progressBar = document.getElementById('uploadProgress');
  
    uploadButton.addEventListener('click', async function () {
      const file = fileInput.files[0];
      if (!file) {
        alert('Please select a file to upload.');
        return;
      }
  
      const CHUNK_SIZE = 10 * 1024 * 1024; // 500MB in bytes
  
      if (file.size <= CHUNK_SIZE) {
        // File is smaller than 500MB, upload directly
        await uploadFile(file, progressBar);
      } else {
        // File is larger than 500MB, split into chunks
        await uploadFileInChunks(file, CHUNK_SIZE, progressBar);
      }
    });
  
    async function uploadFile(file, progressBar) {
      // Request pre-signed URL from backend
      const presignedUrl = await getPresignedUrl(file.name);
  
      // Upload the file using fetch
      const response = await fetch(presignedUrl, {
        method: 'PUT',
        headers: {
          'x-ms-blob-type': 'BlockBlob'
        },
        body: file
      });
  
      if (response.ok) {
        progressBar.value = 100;
        alert('Upload completed successfully.');
      } else {
        alert('Upload failed.');
      }
    }
  
    async function uploadFileInChunks(file, chunkSize, progressBar) {
      const totalChunks = Math.ceil(file.size / chunkSize);
      let offset = 0;
      let blockIds = [];
      let uploadedBytes = 0;
  
      for (let i = 0; i < totalChunks; i++) {
        const chunk = file.slice(offset, offset + chunkSize);
        const blockId = btoa('block' + i).replace(/=/g, '');
        blockIds.push(blockId);
  
        // Request pre-signed URL for this chunk
        const presignedUrl = await getPresignedUrlForBlock(file.name, blockId);
  
        // Upload the chunk
        const response = await fetch(presignedUrl, {
          method: 'PUT',
          headers: {
            'x-ms-blob-type': 'BlockBlob'
          },
          body: chunk
        });
  
        if (response.ok) {
          uploadedBytes += chunk.size;
          progressBar.value = (uploadedBytes / file.size) * 100;
        } else {
          alert('Upload failed on chunk ' + (i + 1));
          return;
        }
  
        offset += chunkSize;
      }
  
      // Commit the blocks
      const commitUrl = await getCommitUrl(file.name);
  
      const blockListXml = '<?xml version="1.0" encoding="utf-8"?><BlockList>' +
        blockIds.map(id => `<Latest>${id}</Latest>`).join('') +
        '</BlockList>';
  
      const commitResponse = await fetch(commitUrl, {
        method: 'PUT',
        headers: {
          'x-ms-blob-content-type': file.type,
          'Content-Type': 'application/xml'
        },
        body: blockListXml
      });
  
      if (commitResponse.ok) {
        progressBar.value = 100;
        alert('Upload completed successfully.');
      } else {
        alert('Failed to commit blocks.');
      }
    }
  
    async function getPresignedUrl(fileName) {
      // Request pre-signed URL from backend
      const response = await fetch(`/api/file/get-presigned-url?filename=${encodeURIComponent(fileName)}`);
      if (response.ok) {
        const data = await response.json();
        return data.url;
      } else {
        alert('Failed to get pre-signed URL.');
        throw new Error('Failed to get pre-signed URL.');
      }
    }
  
    async function getPresignedUrlForBlock(fileName, blockId) {
      // Request pre-signed URL for block from backend
      const response = await fetch(`/api/file/get-presigned-url-for-block?filename=${encodeURIComponent(fileName)}&blockid=${encodeURIComponent(blockId)}`);
      if (response.ok) {
        const data = await response.json();
        return data.url;
      } else {
        alert('Failed to get pre-signed URL for block.');
        throw new Error('Failed to get pre-signed URL for block.');
      }
    }
  
    async function getCommitUrl(fileName) {
      // Request pre-signed URL to commit blocks
      const response = await fetch(`/api/file/get-commit-url?filename=${encodeURIComponent(fileName)}`);
      if (response.ok) {
        const data = await response.json();
        return data.url;
      } else {
        alert('Failed to get commit URL.');
        throw new Error('Failed to get commit URL.');
      }
    }
  });
  