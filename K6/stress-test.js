import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { SharedArray } from 'k6/data';

// Carregar dados para o teste
const usersData = new SharedArray('users data', function() {
    return [
        { userId: 1, name: "John Doe" },
        { userId: 2, name: "Jane Doe" }
    ]; // Exemplo de dados, adapte conforme necessário
});

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
        const params = {
            params: {
                fileName: 'data_1',
            }
        };

        const fileGetResponse = http.get('https://localhost:8080/file', params);

        check(fileGetResponse, {
            'is status 200': (r) => r.status === 200,
            'contains users': (r) => r.json('getFileOutput').users.length > 0,
        });

        sleep(1);
    });
}
