import java.util.*;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.stream.Collector;

public class Main {
    static class Impl implements SamInterface {
        @Override
        public void apply() {
            System.out.println("Impl::apply() called");
        }
    }

    private static void externalApply() {
        System.out.println("Main::externalApply() called");
    }

    public static void main(String[] args) {
        List<String> list = new ArrayList<String>(16) {{
            add("aaa");
            add("bbb");
            add("ccc");
        }};
        AtomicInteger i = new AtomicInteger();
        Map<String, Integer> map = list.stream()
                .collect(Collector.of(HashMap::new, (h, s) -> h.put(s, i.getAndIncrement()), (left, right) -> { left.putAll(right); return left; }));
    }
}
