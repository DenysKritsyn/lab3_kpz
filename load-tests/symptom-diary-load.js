import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5120';
const KEYCLOAK_URL = __ENV.KEYCLOAK_URL || 'http://localhost:8080';

export const options = {
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
  stages: [
    { duration: '2s', target: 20 },  // warm-up to 20 VUs
    { duration: '4s', target: 50 },  // sustained load at 50 VUs
    { duration: '2s', target: 0 },   // ramp-down
  ],
  thresholds: {
    http_req_duration: ['p(95)<300'],
    http_req_failed: ['rate<0.01'],
  },
};

export function setup() {
  const tokenRes = http.post(
    `${KEYCLOAK_URL}/realms/symptom-diary/protocol/openid-connect/token`,
    {
      client_id: 'symptom-diary-client',
      grant_type: 'password',
      username: 'patient1',
      password: 'patient123',
    }
  );

  check(tokenRes, {
    'Keycloak token issued (200)': (r) => r.status === 200,
  });

  return { token: tokenRes.json('access_token') };
}

export default function (data) {
  const params = {
    headers: {
      Authorization: `Bearer ${data.token}`,
      Accept: 'application/json',
      'Accept-Encoding': 'gzip, br',
    },
  };

  const symptomsRes = http.get(
    `${BASE_URL}/api/v1/symptoms?page=1&pageSize=10&sortBy=name&sortDirection=asc`,
    params
  );
  check(symptomsRes, { 'GET /symptoms is 200': (r) => r.status === 200 });

  const entriesRes = http.get(
    `${BASE_URL}/api/v1/diary-entries?page=1&pageSize=10&sortBy=date&sortDirection=desc`,
    params
  );
  check(entriesRes, { 'GET /diary-entries is 200': (r) => r.status === 200 });

  const eventsRes = http.get(
    `${BASE_URL}/api/v1/day-events?page=1&pageSize=10`,
    params
  );
  check(eventsRes, { 'GET /day-events is 200': (r) => r.status === 200 });
}