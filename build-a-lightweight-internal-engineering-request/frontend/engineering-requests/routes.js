import EngineeringRequestDashboard from './pages/EngineeringRequestDashboard.vue';
import MySubmittedRequests from './pages/MySubmittedRequests.vue';
import MySubmissionLink from './pages/MySubmissionLink.vue';
import RequestDetails from './pages/RequestDetails.vue';
import RequestsList from './pages/RequestsList.vue';
import RecurringIssues from './pages/RecurringIssues.vue';
import ReleaseChangeLog from './pages/ReleaseChangeLog.vue';
import RunbookDetails from './pages/RunbookDetails.vue';
import RunbooksList from './pages/RunbooksList.vue';
import SystemRiskDashboard from './pages/SystemRiskDashboard.vue';
import SystemsRegister from './pages/SystemsRegister.vue';
import SubmitRequest from './pages/SubmitRequest.vue';
import TriageRequests from './pages/TriageRequests.vue';
import WeeklyManagementSummary from './pages/WeeklyManagementSummary.vue';

export const engineeringRequestRoutes = [
  {
    path: '/engineering-requests',
    name: 'engineeringRequests',
    component: RequestsList
  },
  {
    path: '/requests/submit',
    name: 'submitRequest',
    component: SubmitRequest
  },
  {
    path: '/requests/submit/:ownerToken',
    name: 'submitRequestWithToken',
    component: SubmitRequest
  },
  {
    path: '/Requests/Submit',
    name: 'submitRequestLegacy',
    component: SubmitRequest
  },
  {
    path: '/Requests/Submit/:ownerToken',
    name: 'submitRequestWithTokenLegacy',
    component: SubmitRequest
  },
  {
    path: '/requests/my-submission-link',
    name: 'mySubmissionLink',
    component: MySubmissionLink
  },
  {
    path: '/requests/triage',
    name: 'triageRequests',
    component: TriageRequests
  },
  {
    path: '/Requests/Triage',
    name: 'triageRequestsLegacy',
    component: TriageRequests
  },
  {
    path: '/requests/my-submitted',
    name: 'mySubmittedRequests',
    component: MySubmittedRequests
  },
  {
    path: '/Requests/MySubmitted',
    name: 'mySubmittedRequestsLegacy',
    component: MySubmittedRequests
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
  },
  {
    path: '/weekly-management-summary',
    name: 'weeklyManagementSummary',
    component: WeeklyManagementSummary
  },
  {
    path: '/recurring-issues',
    name: 'recurringIssues',
    component: RecurringIssues
  },
  {
    path: '/system-risk-dashboard',
    name: 'systemRiskDashboard',
    component: SystemRiskDashboard
  },
  {
    path: '/release-change-log',
    name: 'releaseChangeLog',
    component: ReleaseChangeLog
  },
  {
    path: '/runbooks',
    name: 'runbooks',
    component: RunbooksList
  },
  {
    path: '/runbooks/new',
    name: 'newRunbook',
    component: RunbookDetails
  },
  {
    path: '/runbooks/:id',
    name: 'runbookDetails',
    component: RunbookDetails,
    props: true
  }
];
