<template>
  <main class="er-page">
    <header class="er-page-header">
      <div>
        <h1>System Risk Dashboard</h1>
        <p>Risk, workload, and repeated-issue visibility by system.</p>
      </div>
      <button type="button" @click="loadDashboard">Refresh</button>
    </header>
    <p v-if="error" class="er-error">{{ error }}</p>
    <section class="er-grid">
      <RiskTable title="High-risk systems" :rows="risk?.highRiskSystems || []" />
      <WorkloadTable title="Most open requests" :rows="risk?.systemsWithMostOpenRequests || []" />
      <WorkloadTable title="Most P1/P2 requests" :rows="risk?.systemsWithMostP1P2Requests || []" />
      <RecurringTable title="Repeated issues" :rows="risk?.systemsWithRepeatedIssues || []" />
      <RiskTable title="Known risks" :rows="risk?.systemsWithKnownRisks || []" />
    </section>
  </main>
</template>

<script setup>
import { defineComponent, h, onMounted, ref } from 'vue';
import { engineeringRequestsApi } from '../api';

const risk = ref(null);
const error = ref('');

const RiskTable = defineComponent({
  props: { title: String, rows: Array },
  setup(props) {
    return () => h('article', { class: 'er-panel' }, [
      h('h2', props.title),
      h('table', { class: 'er-table' }, [h('tbody', props.rows?.length
        ? props.rows.map(row => h('tr', { key: row.id }, [
          h('td', row.name),
          h('td', row.criticality),
          h('td', row.knownRisks || 'No risk notes')
        ]))
        : [h('tr', [h('td', { colspan: 3 }, 'None')])])])
    ]);
  }
});

const WorkloadTable = defineComponent({
  props: { title: String, rows: Array },
  setup(props) {
    return () => h('article', { class: 'er-panel' }, [
      h('h2', props.title),
      h('table', { class: 'er-table' }, [h('tbody', props.rows?.length
        ? props.rows.map(row => h('tr', { key: row.systemName }, [
          h('td', row.systemName),
          h('td', `${row.openCount} open`),
          h('td', `${row.p1Count} P1`),
          h('td', `${row.p2Count} P2`)
        ]))
        : [h('tr', [h('td', { colspan: 4 }, 'None')])])])
    ]);
  }
});

const RecurringTable = defineComponent({
  props: { title: String, rows: Array },
  setup(props) {
    return () => h('article', { class: 'er-panel' }, [
      h('h2', props.title),
      h('table', { class: 'er-table' }, [h('tbody', props.rows?.length
        ? props.rows.map(row => h('tr', { key: row.systemName }, [
          h('td', row.systemName),
          h('td', `${row.issueCount} issue(s)`),
          h('td', `${row.totalRecurrences} recurrence(s)`),
          h('td', row.permanentFixNeeded ? 'Permanent fix needed' : '')
        ]))
        : [h('tr', [h('td', { colspan: 4 }, 'None')])])])
    ]);
  }
});

async function loadDashboard() {
  error.value = '';
  try {
    risk.value = await engineeringRequestsApi.getSystemRiskDashboard();
  } catch (err) {
    error.value = err.message;
  }
}

onMounted(loadDashboard);
</script>

<style scoped>
.er-page { padding: 1rem; display: grid; gap: 1rem; }
.er-page-header { display: flex; justify-content: space-between; gap: 1rem; align-items: center; }
h1, h2, p { margin: 0; }
h1 { font-size: 1.5rem; }
h2 { font-size: 1rem; margin-bottom: 0.75rem; }
p { color: #5d6875; margin-top: 0.25rem; }
button { border: 1px solid #cfd6df; border-radius: 4px; padding: 0.55rem 0.65rem; font: inherit; }
.er-grid { display: grid; gap: 0.75rem; grid-template-columns: repeat(auto-fit, minmax(360px, 1fr)); }
.er-panel { border: 1px solid #d9dee5; border-radius: 6px; padding: 0.875rem; background: #fff; overflow-x: auto; }
.er-table { width: 100%; border-collapse: collapse; }
td { border-bottom: 1px solid #e2e7ed; padding: 0.55rem; vertical-align: top; }
.er-error { color: #8b1d10; }
</style>
