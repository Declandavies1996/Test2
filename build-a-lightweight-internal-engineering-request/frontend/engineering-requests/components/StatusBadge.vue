<template>
  <span class="er-badge" :class="badgeClass">{{ value }}</span>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  value: {
    type: String,
    required: true
  }
});

const badgeClass = computed(() => `er-badge--${props.value.toLowerCase()}`);
</script>

<style scoped>
.er-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 2.5rem;
  min-height: 1.5rem;
  padding: 0 0.5rem;
  border-radius: 4px;
  font-size: 0.8125rem;
  font-weight: 700;
  white-space: nowrap;
  background: #eceff3;
  color: #263238;
}

.er-badge--p1,
.er-badge--incoming,
.er-badge--bug {
  background: #ffe1dc;
  color: #8b1d10;
}

.er-badge--p2,
.er-badge--inprogress,
.er-badge--validation,
.er-badge--investigation {
  background: #fff1c7;
  color: #6b4b00;
}

.er-badge--p3,
.er-badge--planned,
.er-badge--waiting,
.er-badge--support,
.er-badge--importexport {
  background: #dceeff;
  color: #174c75;
}

.er-badge--p4,
.er-badge--done,
.er-badge--feature {
  background: #dff3e5;
  color: #17612d;
}

.er-badge--technicaldebt {
  background: #eadff5;
  color: #4b2b6b;
}
</style>
