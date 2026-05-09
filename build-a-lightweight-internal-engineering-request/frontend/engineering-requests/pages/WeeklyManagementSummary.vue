<template>
  <main class="er-page">
    <header class="er-page-header">
      <div>
        <h1>Weekly Management Summary</h1>
        <p>Copy-ready operational update for engineering support work.</p>
      </div>
      <label class="er-check"><input v-model="filters.allRequests" type="checkbox" @change="loadSummary" /> All requests</label>
      <button type="button" @click="copySummary">Copy summary</button>
    </header>

    <p v-if="error" class="er-error">{{ error }}</p>

    <textarea class="er-copy-box" :value="copyText" readonly rows="14"></textarea>

    <section class="er-grid">
      <article class="er-panel">
        <h2>Talking Points</h2>
        <ul>
          <li v-for="point in summary?.talkingPoints || []" :key="point">{{ point }}</li>
        </ul>
      </article>
      <article class="er-panel">
        <h2>Systems Causing Most Work</h2>
        <table class="er-table">
          <tbody>
            <tr v-for="system in summary?.systemsCausingMostWork || []" :key="system.systemName">
              <td>{{ system.systemName }}</td>
              <td>{{ system.openCount }} open</td>
              <td>{{ system.p1Count }} P1</td>
              <td>{{ system.p2Count }} P2</td>
            </tr>
          </tbody>
        </table>
      </article>
    </section>

    <section class="er-grid">
      <article class="er-panel">
        <h2>Completed This Week</h2>
        <RequestMiniTable :items="summary?.completedRequestsThisWeek || []" />
      </article>
      <article class="er-panel">
        <h2>New This Week</h2>
        <RequestMiniTable :items="summary?.newRequestsThisWeek || []" />
      </article>
    </section>

    <section class="er-grid">
      <article class="er-panel">
        <h2>Open P1/P2</h2>
        <RequestMiniTable :items="summary?.openP1P2Requests || []" />
      </article>
      <article class="er-panel">
        <h2>Blocked / Waiting</h2>
        <table class="er-table">
          <tbody>
            <tr v-for="request in summary?.blockedWaitingRequests || []" :key="request.id">
              <td>#{{ request.id }}</td>
              <td>{{ request.title }}</td>
              <td>{{ request.reason || 'No note recorded' }}</td>
            </tr>
          </tbody>
        </table>
      </article>
    </section>
  </main>
</template>

<script setup>
import { computed, defineComponent, h, onMounted, reactive, ref } from 'vue';
import { engineeringRequestsApi } from '../api';

const summary = ref(null);
const error = ref('');
const filters = reactive({ allRequests: false });

const RequestMiniTable = defineComponent({
  props: { items: { type: Array, default: () => [] } },
  setup(props) {
    return () => h('table', { class: 'er-table' }, [
      h('tbody', props.items.length
        ? props.items.map(item => h('tr', { key: item.id }, [
          h('td', `#${item.id}`),
          h('td', item.title),
          h('td', item.systemName),
          h('td', item.priority),
          h('td', item.status)
        ]))
        : [h('tr', [h('td', { colspan: 5 }, 'None')])])
    ]);
  }
});

const copyText = computed(() => {
  if (!summary.value) return '';
  const lines = [
    `Weekly CAE Support Summary (${formatDate(summary.value.weekStart)} to ${formatDate(summary.value.weekEnd)})`,
    '',
    'Talking points:',
    ...summary.value.talkingPoints.map(x => `- ${x}`),
    '',
    `Completed this week: ${summary.value.completedRequestsThisWeek.length}`,
    `New this week: ${summary.value.newRequestsThisWeek.length}`,
    `Open P1/P2: ${summary.value.openP1P2Requests.length}`,
    `Blocked/waiting: ${summary.value.blockedWaitingRequests.length}`,
    '',
    'Systems causing most work:',
    ...summary.value.systemsCausingMostWork.map(x => `- ${x.systemName}: ${x.openCount} open (${x.p1Count} P1, ${x.p2Count} P2)`)
  ];
  return lines.join('\n');
});

async function loadSummary() {
  error.value = '';
  try {
    summary.value = await engineeringRequestsApi.getWeeklyManagementSummary(filters);
  } catch (err) {
    error.value = err.message;
  }
}

async function copySummary() {
  await navigator.clipboard.writeText(copyText.value);
}

function formatDate(value) {
  return new Date(value).toLocaleDateString();
}

onMounted(loadSummary);
</script>

<style scoped>
.er-page { padding: 1rem; display: grid; gap: 1rem; }
.er-page-header { display: flex; justify-content: space-between; gap: 1rem; align-items: center; }
h1, h2, p { margin: 0; }
h1 { font-size: 1.5rem; }
h2 { font-size: 1rem; margin-bottom: 0.75rem; }
p { color: #5d6875; margin-top: 0.25rem; }
button, textarea { border: 1px solid #cfd6df; border-radius: 4px; padding: 0.55rem 0.65rem; font: inherit; }
.er-check { display: inline-flex; align-items: center; gap: 0.4rem; }
.er-copy-box { width: 100%; box-sizing: border-box; }
.er-grid { display: grid; gap: 0.75rem; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); }
.er-panel { border: 1px solid #d9dee5; border-radius: 6px; padding: 0.875rem; background: #fff; overflow-x: auto; }
.er-table { width: 100%; border-collapse: collapse; }
td { border-bottom: 1px solid #e2e7ed; padding: 0.55rem; vertical-align: top; }
.er-error { color: #8b1d10; }
</style>
