import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { SharedArray } from 'k6/data';

const fileData = open('file-to-upload.txt', 'b');

export let options = {
    stages: [
        { duration: '30s', target: 100 }, 
        { duration: '30s', target: 200 }, 
        { duration: '30s', target: 50 },
    ],
};

export default function () {
    sleep(1);
    group('File Sync Endpoint Test', function () {
        const url = 'http://nginx/file/sync'; // Atualizado para usar Nginx

        const payload = {
            file: http.file(fileData, 'file-to-upload.txt', 'text/plain'),
        };

        const headers = {
            accept: 'application/json',
        };

        let maxRetries = 2; // Máximo de tentativas ao receber HTTP 429
        let retryDelay = 61; // Tempo inicial de espera (segundos)
        let fileSyncResponse;

        for (let i = 0; i < maxRetries; i++) {
            fileSyncResponse = http.post(url, payload, { headers });

            if (fileSyncResponse.status !== 429) break;

            console.warn(`Rate limit exceeded. Retrying in ${retryDelay}s...`);
            sleep(retryDelay); // Espera antes de tentar novamente
            retryDelay *= 2; // Aumenta o tempo de espera exponencialmente
        }

        if (fileSyncResponse.status === 429) {
            console.error('Max retries reached. Still hitting rate limit.');
            return;
        }

        check(fileSyncResponse, {
            'is status 200': (r) => r.status === 200,
            'is successful response': (r) => {
                try {
                    return r.json('fileSyncOutput')?.success === true;
                } catch (err) {
                    console.error(`Error parsing response JSON: ${err.message}`);
                    return false;
                }
            },
        });

        sleep(1);
    });

    group('Get File Endpoint Test 1', function () {
        const url = 'http://nginx/file?fileName=data_1'; // Atualizado para usar Nginx
        let fileGetResponse;

        try {
            fileGetResponse = http.get(url);
        } catch (error) {
            console.error(`Failed to send request to ${url}: ${error.message}`);
            return;
        }

        if (!fileGetResponse || !fileGetResponse.body) {
            console.error(`Response from ${url} is null or has no body.`);
            return;
        }

        const validationResult = check(fileGetResponse, {
            'is status 200': (r) => r.status === 200,
            'contains getFileOutput': (r) => !!r.json('getFileOutput'),
            'contains users': (r) => {
                const output = r.json('getFileOutput');
                return output && Array.isArray(output.users) && output.users.length > 0;
            },
        });

        if (fileGetResponse.status !== 200) {
            console.error('Request failed with status:', fileGetResponse.status);
        }

        if (!validationResult) {
            console.error(`Validation failed for response: ${fileGetResponse.body}`);
        }

        sleep(1);
    });

    group('Get File Endpoint Test 2', function () {
        const url = 'http://nginx/file?fileName=data_2'; // Atualizado para usar Nginx
        let fileGetResponse;

        try {
            fileGetResponse = http.get(url);
        } catch (error) {
            console.error(`Failed to send request to ${url}: ${error.message}`);
            return;
        }

        if (!fileGetResponse || !fileGetResponse.body) {
            console.error(`Response from ${url} is null or has no body.`);
            return;
        }

        const validationResult = check(fileGetResponse, {
            'is status 200': (r) => r.status === 200,
            'contains getFileOutput': (r) => !!r.json('getFileOutput'),
            'contains users': (r) => {
                const output = r.json('getFileOutput');
                return output && Array.isArray(output.users) && output.users.length > 0;
            },
        });

        if (fileGetResponse.status !== 200) {
            console.error('Request failed with status:', fileGetResponse.status);
        }

        if (!validationResult) {
            console.error(`Validation failed for response: ${fileGetResponse.body}`);
        }

        sleep(1);
    });
}
