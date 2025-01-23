import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { SharedArray } from 'k6/data';

export let options = {
    stages: [
        { duration: '30s', target: 10 }, // 10 usuários em 30 segundos
        { duration: '1m', target: 100 }, // 100 usuários em 1 minuto
        { duration: '30s', target: 0 },  // diminuir para 0 usuários em 30 segundos
    ],
};

export default function () {
    // group('File Sync Endpoint Test', function () {
    //     const fileSyncResponse = http.post('http://localhost:8080/file/sync', {
    //         file: open('file-to-upload.txt', 'b'),
    //     }, {
    //         headers: { 'Content-Type': 'multipart/form-data' },
    //     });
    //
    //     check(fileSyncResponse, {
    //         'is status 200': (r) => r.status === 200,
    //         'is successful response': (r) => r.json('fileSyncOutput').success === true,
    //     });
    //
    //     sleep(1);
    // });

    group('Get File Endpoint Test', function () {
        const url = 'http://ordersyncapi:8080/file?fileName=data_1';
        let fileGetResponse;

        try {
            fileGetResponse = http.get(url);
        } catch (error) {
            console.error(`Failed to send request to ${url}: ${error.message}`);
            return; // Aborta o teste caso não consiga enviar a requisição.
        }

        // Verifica se a resposta é válida antes de realizar as validações.
        if (!fileGetResponse || !fileGetResponse.body) {
            console.error(`Response from ${url} is null or has no body.`);
            return; // Aborta o teste, já que não há dados para validar.
        }

        // Aplica as validações sobre a resposta.
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

        // Loga o erro caso alguma validação falhe.
        if (!validationResult) {
            console.error(`Validation failed for response: ${fileGetResponse.body}`);
        }

        sleep(1);
    });
}
