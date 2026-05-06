import EngineeringRequestDashboard from './pages/EngineeringRequestDashboard.vue';
import RequestDetails from './pages/RequestDetails.vue';
import RequestsList from './pages/RequestsList.vue';
import SystemsRegister from './pages/SystemsRegister.vue';

export const engineeringRequestRoutes = [
  {
    path: '/engineering-requests',
    name: 'engineeringRequests',
    component: RequestsList
  },
  {
    path: '/engineering-requests/:id',
    name: 'engineeringRequestDetails',
    component: RequestDetails,
    props: true
  },
  {
    path: '/engineering-systems',
    name: 'engineeringSystems',
    component: SystemsRegister
  },
  {
    path: '/engineering-requests-summary',
    name: 'engineeringRequestsSummary',
    component: EngineeringRequestDashboard
  }
];
