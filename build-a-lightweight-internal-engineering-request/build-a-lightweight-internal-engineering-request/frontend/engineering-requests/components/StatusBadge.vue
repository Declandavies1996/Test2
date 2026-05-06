<template>
  <span class="er-badge" :class="`er-badge--${normalized}`">{{ value }}</span>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  value: {
    type: String,
    required: true
  }
});

const normalized = computed(() => props.value.toLowerCase());
</script>

<style scoped>
.er-badge {
  display: inline-flex;
  align-items: center;
  min-height: 1.5rem;
  padding: 0 0.5rem;
  border-radius: 4px;
  font-size: 0.8125rem;
  font-weight: 600;
  background: #eceff3;
  color: #263238;
}

.er-badge--p1,
.er-badge--incoming {
  background: #ffe1dc;
  color: #8b1d10;
}

.er-badge--p2,
.er-badge--inprogress {
  background: #fff1c7;
  color: #6b4b00;
}

.er-badge--p3,
.er-badge--planned,
.er-badge--waiting {
  background: #dceeff;
  color: #174c75;
}

.er-badge--p4,
.er-badge--done {
  background: #dff3e5;
  color: #17612d;
}
</style>
