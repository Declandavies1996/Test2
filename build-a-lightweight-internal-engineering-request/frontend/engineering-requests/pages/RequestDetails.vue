<template>
  <main class="er-page">
    <button type="button" class="er-link" @click="router.back()">Back</button>

    <p v-if="error" class="er-error">{{ error }}</p>

    <section v-if="request" class="er-detail">
      <header class="er-detail-header">
        <div>
          <h1>{{ request.title }}</h1>
          <div class="er-meta">
            <StatusBadge :value="request.priority" />
            <StatusBadge :value="request.status" />
            <span>{{ request.systemName }}</span>
            <span>{{ request.type }}</span>
          </div>
        </div>
        <button type="button" class="er-primary" @click="saveRequest">Save changes</button>
      </header>

      <div class="er-form-grid">
        <label>
          Title
          <input v-model.trim="form.title" required />
        </label>
        <label>
          System
          <select v-model="form.systemName">
            <option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option>
          </select>
        </label>
        <label>
          Priority
          <select v-model="form.priority">
            <option v-for="priority in priorities" :key="priority.value" :value="priority.value">
              {{ priority.label }}
            </option>
          </select>
        </label>
        <label>
          Status
          <select v-model="form.status">
            <option v-for="status in statuses" :key="status" :value="status">{{ status }}</option>
          </select>
        </label>
        <label>
          Type
          <select v-model="form.type">
            <option v-for="type in requestTypes" :key="type" :value="type">{{ type }}</option>
          </select>
        </label>
        <label>
          Requested by
          <input v-model.trim="form.requestedBy" />
        </label>
        <label>
          Department
          <input v-model.trim="form.department" />
        </label>
      </div>

      <label>
        Description
        <textarea v-model.trim="form.description" rows="5"></textarea>
      </label>

      <label>
        Current notes
        <textarea v-model.trim="form.notes" rows="4"></textarea>
      </label>

      <RequestGuidance />

      <section class="er-notes">
        <h2>Linked runbooks</h2>
        <form class="er-note-form" @submit.prevent="linkRunbook">
          <select v-model="selectedRunbookId">
            <option value="" disabled>Select runbook</option>
            <option v-for="runbook in availableRunbooks" :key="runbook.id" :value="runbook.id">
              {{ runbook.title }} ({{ runbook.systemName }} - {{ runbook.category }})
            </option>
          </select>
          <div class="er-actions">
            <input v-model.trim="linkChangedBy" placeholder="Linked by" />
            <button type="submit" class="er-primary" :disabled="!selectedRunbookId">Link runbook</button>
          </div>
        </form>

        <article v-for="runbook in request.linkedRunbooks" :key="runbook.id" class="er-note">
          <p>
            <RouterLink :to="{ name: 'runbookDetails', params: { id: runbook.runbookId } }">
              {{ runbook.title }}
            </RouterLink>
          </p>
          <span>{{ runbook.systemName }} - {{ runbook.category }} - linked {{ formatDateTime(runbook.linkedDate) }}</span>
          <div class="er-actions er-actions--left">
            <button type="button" @click="unlinkRunbook(runbook.runbookId)">Unlink</button>
          </div>
        </article>
        <p v-if="request.linkedRunbooks.length === 0" class="er-empty">No runbooks linked yet.</p>
      </section>

      <section class="er-notes">
        <h2>Notes</h2>
        <form class="er-note-form" @submit.prevent="addNote">
          <textarea v-model.trim="noteText" rows="3" placeholder="Add a dated progress note"></textarea>
          <div class="er-actions">
            <input v-model.trim="createdBy" placeholder="Created by" />
            <button type="submit" class="er-primary">Add note</button>
          </div>
        </form>

        <article v-for="note in request.requestNotes" :key="note.id" class="er-note">
          <p>{{ note.noteText }}</p>
          <span>{{ note.createdBy || 'Unknown' }} - {{ formatDateTime(note.createdDate) }}</span>
        </article>
        <p v-if="request.requestNotes.length === 0" class="er-empty">No notes yet.</p>
      </section>

      <section class="er-notes">
        <h2>Attachments</h2>
        <form class="er-note-form" @submit.prevent="uploadAttachment">
          <input type="file" accept=".msg,.pdf,.xlsx,.xls,.csv,.txt,.log,.png,.jpg,.jpeg,.zip" @change="setFile" />
          <div class="er-actions">
            <input v-model.trim="uploadedBy" placeholder="Uploaded by" />
            <button type="submit" class="er-primary" :disabled="!selectedFile">Upload</button>
          </div>
        </form>

        <article v-for="attachment in request.attachments" :key="attachment.id" class="er-note">
          <p>
            <a :href="attachmentUrl(attachment.id)" target="_blank" rel="noreferrer">
              {{ attachment.fileName }}
            </a>
          </p>
          <span>{{ attachment.uploadedBy || 'Unknown' }} - {{ formatDateTime(attachment.uploadedDate) }}</span>
        </article>
        <p v-if="request.attachments.length === 0" class="er-empty">No attachments uploaded yet.</p>
      </section>

      <section class="er-notes">
        <h2>Timeline</h2>
        <article v-for="item in request.history" :key="item.id" class="er-note">
          <p>{{ timelineText(item) }}</p>
          <span>{{ item.changedBy || 'System' }} - {{ formatDateTime(item.changedDate) }}</span>
        </article>
        <p v-if="request.history.length === 0" class="er-empty">No timeline entries yet.</p>
      </section>
    </section>
  </main>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { engineeringRequestsApi } from '../api';
import { priorities, requestTypes, statuses } from '../constants';
import RequestGuidance from '../components/RequestGuidance.vue';
import StatusBadge from '../components/StatusBadge.vue';

const props = defineProps({
  id: {
    type: [String, Number],
    required: true
  }
});

const router = useRouter();
const request = ref(null);
const systems = ref([]);
const allRunbooks = ref([]);
const noteText = ref('');
const createdBy = ref('');
const uploadedBy = ref('');
const selectedFile = ref(null);
const selectedRunbookId = ref('');
const linkChangedBy = ref('');
const error = ref('');

const form = reactive({
  title: '',
  description: '',
  systemName: '',
  requestedBy: '',
  department: '',
  priority: 'P3',
  status: 'Incoming',
  type: 'Support',
  notes: ''
});

const availableRunbooks = computed(() => {
  if (!request.value) return allRunbooks.value;
  const linkedIds = new Set(request.value.linkedRunbooks.map(x => x.runbookId));
  return allRunbooks.value.filter(x => !linkedIds.has(x.id));
});

function syncForm(data) {
  Object.assign(form, {
    title: data.title,
    description: data.description || '',
    systemName: data.systemName,
    requestedBy: data.requestedBy || '',
    department: data.department || '',
    priority: data.priority,
    status: data.status,
    type: data.type,
    notes: data.notes || ''
  });
}

async function loadRequest() {
  request.value = await engineeringRequestsApi.getRequest(props.id);
  syncForm(request.value);
}

async function loadRunbooks() {
  allRunbooks.value = await engineeringRequestsApi.getRunbooks();
}

async function saveRequest() {
  error.value = '';
  try {
    await engineeringRequestsApi.updateRequest(props.id, { ...form });
    await loadRequest();
  } catch (err) {
    error.value = err.message;
  }
}

async function addNote() {
  if (!noteText.value) return;

  error.value = '';
  try {
    await engineeringRequestsApi.addNote(props.id, {
      noteText: noteText.value,
      createdBy: createdBy.value
    });
    noteText.value = '';
    await loadRequest();
  } catch (err) {
    error.value = err.message;
  }
}

function setFile(event) {
  selectedFile.value = event.target.files?.[0] || null;
}

async function uploadAttachment() {
  if (!selectedFile.value) return;

  error.value = '';
  try {
    await engineeringRequestsApi.uploadAttachment(props.id, selectedFile.value, uploadedBy.value);
    selectedFile.value = null;
    await loadRequest();
  } catch (err) {
    error.value = err.message;
  }
}

async function linkRunbook() {
  if (!selectedRunbookId.value) return;

  error.value = '';
  try {
    await engineeringRequestsApi.linkRunbookToRequest(props.id, Number(selectedRunbookId.value), linkChangedBy.value);
    selectedRunbookId.value = '';
    await loadRequest();
  } catch (err) {
    error.value = err.message;
  }
}

async function unlinkRunbook(runbookId) {
  error.value = '';
  try {
    await engineeringRequestsApi.unlinkRunbookFromRequest(props.id, runbookId, linkChangedBy.value);
    await loadRequest();
  } catch (err) {
    error.value = err.message;
  }
}

function attachmentUrl(id) {
  return engineeringRequestsApi.attachmentUrl(id);
}

function timelineText(item) {
  if (item.actionType === 'RequestCreated') return `Request created: ${item.newValue || ''}`;
  if (item.actionType === 'NoteAdded') return 'Note added';
  if (item.actionType === 'AttachmentUploaded') return `Attachment uploaded: ${item.newValue || ''}`;
  if (item.actionType === 'StatusChanged') return `Status changed from ${item.oldValue} to ${item.newValue}`;
  if (item.actionType === 'PriorityChanged') return `Priority changed from ${item.oldValue} to ${item.newValue}`;
  if (item.actionType === 'RunbookLinked') return `Runbook linked: ${item.newValue || ''}`;
  if (item.actionType === 'RunbookUnlinked') return `Runbook unlinked: ${item.oldValue || ''}`;
  if (item.actionType === 'UserSubmitted') return `User submitted request: ${item.newValue || ''}`;
  if (item.actionType === 'RequestTriaged') return 'Request triaged';
  if (item.actionType === 'SystemAssigned') return `System assigned from ${item.oldValue || 'blank'} to ${item.newValue || 'blank'}`;
  if (item.actionType === 'PriorityAssigned') return `Priority assigned from ${item.oldValue} to ${item.newValue}`;
  if (item.actionType === 'TypeAssigned') return `Type assigned from ${item.oldValue} to ${item.newValue}`;
  if (item.actionType === 'OwnerAssigned') return `Owner assigned from ${item.oldValue || 'blank'} to ${item.newValue || 'blank'}`;
  return item.actionType;
}

function formatDateTime(value) {
  return new Date(value).toLocaleString();
}

onMounted(async () => {
  systems.value = await engineeringRequestsApi.getSystems();
  await loadRunbooks();
  await loadRequest();
});
</script>

<style scoped>
.er-page {
  padding: 1rem;
}

.er-link {
  background: transparent;
  border: 0;
  color: #1f5e8c;
  padding: 0;
  margin-bottom: 1rem;
}

.er-detail,
.er-notes {
  display: grid;
  gap: 1rem;
}

.er-detail-header,
.er-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
}

.er-actions--left {
  justify-content: flex-start;
  margin-top: 0.5rem;
}

h1,
h2 {
  margin: 0;
}

h1 {
  font-size: 1.5rem;
}

h2 {
  font-size: 1rem;
}

.er-meta {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
  margin-top: 0.5rem;
  color: #5d6875;
}

.er-form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(190px, 1fr));
  gap: 0.75rem;
}

label,
.er-note-form {
  display: grid;
  gap: 0.35rem;
}

input,
select,
textarea,
button {
  border: 1px solid #cfd6df;
  border-radius: 4px;
  padding: 0.55rem 0.65rem;
  font: inherit;
}

.er-primary {
  background: #1f5e8c;
  color: #ffffff;
  border-color: #1f5e8c;
  font-weight: 600;
}

.er-note {
  border: 1px solid #d9dee5;
  border-radius: 6px;
  padding: 0.875rem;
}

.er-note p {
  margin: 0 0 0.5rem;
}

.er-note span,
.er-empty {
  color: #6b7580;
  font-size: 0.875rem;
}

.er-error {
  color: #8b1d10;
}
</style>
